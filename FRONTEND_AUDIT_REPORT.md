# تقرير مراجعة الفرونت

التاريخ: 2026-05-01  
الفرع: `master`  
نتيجة السحب: أمر `git pull origin master` رجع `Already up to date`.

## ملخص التحقق

- بناء الفرونت عبر Docker نجح باستخدام `docker compose build client`.
- داخل Docker تم تشغيل `tsc -b && vite build` بنجاح.
- تحذيرات البناء:
  - بيانات Browserslist قديمة منذ 11 شهر.
  - بعض chunks أكبر من 500 KB؛ أكبر ملف JS خرج بحجم تقريبي `798.75 KB` بعد التصغير.
- محليا داخل جلسة PowerShell الحالية، أوامر `node` و`npm` و`npx` غير موجودة في PATH، لذلك لم أستطع تشغيل `npm run lint` أو `npm test` مباشرة من الجهاز.
- لم يتم تعديل كود الفرونت. الملف الوحيد المضاف هو هذا التقرير.

## مشاكل عالية الأولوية

### 1. تنقل الموبايل ناقص

السايدبار على الديسكتوب يحتوي أغلب أقسام النظام في `src/components/dashboard/Sidebar.tsx:22-35`: الخدمات، العملاء، المشاريع، الفواتير، المدفوعات، المصاريف، التسعير، تحليل العميل، الربح، التحليل، التدفق النقدي، ومحلل الذكاء.

لكن شريط الموبايل في `src/components/dashboard/DashboardLayout.tsx:90-95` يعرض 5 روابط فقط: الرئيسية، المشاريع، العملاء، الفواتير، المصاريف. صفحات مهمة مثل الخدمات، المدفوعات، التسعير، تحليل العميل، كشف الربح، أين ذهب الربح، التدفق النقدي، ومحلل الذكاء لا تظهر مباشرة للمستخدم على الموبايل.

يوجد أيضا مشكلة حساب على الشاشات الصغيرة: قائمة الحساب مخفية إلى أن يصل العرض إلى `sm` عبر `hidden ... sm:flex` في `src/components/dashboard/DashboardLayout.tsx:320`، والسايدبار مخفي على الموبايل. هذا يجعل الملف الشخصي، تغيير كلمة المرور، وتسجيل الخروج صعبة أو غير متاحة على أصغر الشاشات.

الأثر: مستخدم الموبايل قد لا يستطيع الوصول لأقسام أساسية أو لإجراءات الحساب.

الاقتراح: إضافة قائمة موبايل كاملة، أو زر "المزيد" يفتح Sheet/Menu يحتوي كل الروابط وإجراءات الحساب.

### 2. ميزة تحليل مخاطر العميل تعرض نموذج AI مختلف عن المستخدم فعليا

الفرونت يعرض الميزة على أنها `Gemma 4` وفيه ثابت `GEMMA_MODEL = "google/gemma-4-31b-it"` في `src/pages/dashboard/ClientRiskAnalyzer.tsx:77`، والنصوص تعرض Gemma في `src/pages/dashboard/ClientRiskAnalyzer.tsx:569` و`641` و`648`.

لكن الباك إند يستخدم فعليا `OpenRouterModel = "openai/gpt-4o-mini"` في `src/SalamHack.Api/Controllers/ClientRiskController.cs:15`.

كمان الفرونت يرسل الطلب إلى `/api/v1/clientrisk/analyze` بدون access token في `src/pages/dashboard/ClientRiskAnalyzer.tsx:178-184`، والباك إند معرف endpoint بدون `[Authorize]` وبـ `public-read` في `src/SalamHack.Api/Controllers/ClientRiskController.cs:20-25`.

الأثر: المستخدم يرى اسم نموذج غير صحيح، وendpoint مكلف للذكاء الاصطناعي قابل للاستدعاء بشكل عام رغم أن الشاشة نفسها داخل الداشبورد.

الاقتراح: توحيد اسم النموذج المعروض مع النموذج الحقيقي، أو إرجاع بيانات النموذج من API. وإذا الميزة مخصصة للمستخدمين المسجلين، يجب إضافة Authorization وإرسال التوكن.

### 3. إعداد تاريخ الرصيد الافتتاحي في التدفق النقدي لا يؤثر

في صفحة التدفق النقدي، الحالة `openingBalanceDateUtc` موجودة في `src/pages/dashboard/Cashflow.tsx:14` ويتم تحديثها في `src/pages/dashboard/Cashflow.tsx:104-107`.

لكن القيمة لا يتم تمريرها لأي من المكونات في `src/pages/dashboard/Cashflow.tsx:134-145`.

الدالة `fetchCashFlowForecast` تدعم أصلا `openingBalanceDateUtc` في `src/lib/reports.ts:92-101`، لكن كل الاستدعاءات الحالية تمرر فقط `asOfUtc` و`openingBalance`.

يوجد أيضا عدم تطابق في type: الفرونت يعرف `openingBalance.balanceUtc` في `src/lib/reports.ts:17`، بينما موديل الباك إند يستخدم `EffectiveAtUtc` في `src/SalamHack.Application/Features/Reports/Models/CashOpeningBalanceDto.cs:3-5`.

الأثر: المستخدم يغير التاريخ من الواجهة لكن الحسابات لا تتغير، والـ DTO في الفرونت مضلل.

الاقتراح: تمرير `openingBalanceDateUtc` لكل طلبات التدفق النقدي، أو الأفضل جلب البيانات مرة واحدة من صفحة `Cashflow.tsx` وتمرير النتيجة للمكونات. كذلك تحديث type ليستخدم `effectiveAtUtc`.

### 4. صفحة التدفق النقدي تعمل نفس طلب API أربع مرات

كل مكون داخل صفحة التدفق النقدي يستدعي `fetchCashFlowForecast` لوحده:

- `src/components/dashboard/CashFlowSummary.tsx:19`
- `src/components/dashboard/CashFlowProjection.tsx:20`
- `src/components/dashboard/PendingInvoices.tsx:31`
- `src/components/dashboard/RecurringExpenses.tsx:31`

الأثر: تحميل واحد للصفحة ينتج أربع طلبات متكررة لنفس البيانات، مع loading/error states متفرقة وضغط زائد على الباك إند.

الاقتراح: جلب التقرير مرة واحدة في `Cashflow.tsx` وتمريره للمكونات، أو استخدام React Query بمفتاح مشترك.

### 5. التنبيهات تظهر كنقطة فقط ولا يوجد Inbox فعلي

الواجهة تتصل بـ SignalR في `src/components/dashboard/DashboardLayout.tsx:210`، وعند وصول `ReceiveNotification` يتم فقط تشغيل `setHasNewNotifications(true)` في `src/components/dashboard/DashboardLayout.tsx:217-219`.

زر الجرس في `src/components/dashboard/DashboardLayout.tsx:353-355` يمسح النقطة الحمراء فقط. لا يوجد عرض لقائمة التنبيهات، ولا تحميل للتنبيهات، ولا mark as read.

الباك إند يحتوي endpoints جاهزة للتنبيهات في `src/SalamHack.Api/Controllers/NotificationsController.cs:17` و`33` و`51`.

الأثر: التنبيهات تصل، لكن المستخدم لا يستطيع قراءتها أو إدارتها من الواجهة.

الاقتراح: بناء popover أو sheet للتنبيهات يحتوي unread list، حالة تحميل، حالة خطأ، وmark as read.

## مشاكل متوسطة الأولوية

### 6. Routes مكررة داخل الداشبورد

في `src/App.tsx` توجد routes فعلية لـ `payments`, `pricing`, `profit`, `breakdown`, `cashflow`, و`ai` في الأسطر `87-93`.

بعدها مباشرة توجد redirects لنفس المسارات في `src/App.tsx:95-100`.

هذه redirects عمليا dead code لأن route الأول يطابق قبلها.

الأثر: يربك أي مطور لاحق؛ قد يظن أن هذه الميزات معطلة أو يتم تحويلها بينما الواقع غير ذلك.

الاقتراح: حذف redirects غير المستخدمة، أو تطبيق التحويل/الحجب بشكل واحد وواضح.

### 7. صفحة `ClientRiskResultPage` موجودة لكنها غير موصولة

الصفحة موجودة في `src/pages/dashboard/ClientRiskResult.tsx:45`، لكن `src/App.tsx:94` يوجه فقط `/dashboard/client-risk` إلى `ClientRiskAnalyzerPage`.

حاليا النتيجة تظهر داخل صفحة التحليل نفسها، لذلك صفحة النتيجة المنفصلة غير قابلة للوصول.

الأثر: كود زائد ومسار UX غير واضح.

الاقتراح: إما حذف الصفحة غير المستخدمة، أو إضافة route واضح مثل `/dashboard/client-risk/result` والتنقل إليه عند التحليل.

### 8. نموذج التواصل في اللاندنج لا يرسل شيئا

النموذج في `src/components/landing/ContactSection.tsx:53` يعمل فقط `e.preventDefault()`.

الحقول في `src/components/landing/ContactSection.tsx:55-60` غير مربوطة بإرسال، ولا يوجد API call، ولا mailto fallback، ولا رسالة نجاح أو خطأ.

الأثر: المستخدم يملأ النموذج ويضغط إرسال بدون أي نتيجة.

الاقتراح: ربط النموذج بـ API أو خدمة بريد، أو تعطيله/إخفاؤه إلى أن تصبح العملية جاهزة.

### 9. Vite proxy المحلي لا يغطي SignalR hubs

`vite.config.ts:11-13` يعمل proxy فقط لمسار `/api` إلى `http://localhost:5134`.

لكن الفرونت يتصل بـ `${API_BASE_URL}/hubs/notifications` في `src/components/dashboard/DashboardLayout.tsx:210`. عند تشغيل Vite بدون `VITE_API_BASE_URL`، سيحاول الاتصال بـ `/hubs/notifications` على Vite نفسه، وليس الباك إند.

كذلك `.env.example` في جذر المشروع يحتوي `CLIENT_HTTP_PORT` و`API_HTTP_PORT`، لكنه لا يوثق `VITE_API_BASE_URL`.

الأثر: التنبيهات قد تعمل في Docker/nginx لكنها تفشل في التطوير المحلي.

الاقتراح: إضافة proxy لمسار `/hubs` في Vite، وتوثيق `VITE_API_BASE_URL`.

### 10. مسار تأكيد التسجيل هش عند Refresh

صفحة `RegisterVerify` تعتمد بالكامل على `location.state` لجلب `registrationData` في `src/pages/RegisterVerify.tsx:35`.

إذا المستخدم عمل refresh على `/register/verify` أو فتح الصفحة في تبويب جديد، سيتم تحويله إلى `/register` في `src/pages/RegisterVerify.tsx:40-41`.

كذلك لا يظهر في الصفحة خيار إعادة إرسال رمز التحقق.

الأثر: المستخدم قد يفقد تدفق التسجيل بعد وصول OTP.

الاقتراح: إضافة مسار استعادة واضح: إدخال البريد لإعادة إرسال الكود، countdown، ورسالة واضحة إذا انتهت الجلسة.

### 11. فشل refresh token قد يترك صفحة محمية فارغة

`RequireAuth` يعرض `null` أثناء التحقق في `src/App.tsx:64`، ويستدعي `refreshAccessToken()` في `src/App.tsx:51-53`.

الدالة `refreshAccessToken` في `src/lib/auth.ts:153-176` لا تمسك network exceptions. إذا فشل الطلب قبل وصول response، قد لا يتم تحديث حالة `RequireAuth` وتبقى الصفحة فارغة.

الأثر: عند سقوط الشبكة أو API أثناء refresh، المستخدم قد يرى شاشة بيضاء بدل رسالة أو تحويل تسجيل دخول.

الاقتراح: إضافة `try/catch` حول refresh، ومسار fallback واضح: retry أو clear session ثم login.

## فجوات جودة وصيانة

### 12. حجم الـ bundle يحتاج route-level splitting

البناء نجح، لكن Vite حذر من chunks أكبر من 500 KB، وأكبر JS chunk كان حوالي `798.75 KB`.

`src/App.tsx:7-34` يستورد كل الصفحات بشكل static، وهذا يسحب أقسام كثيرة من الداشبورد مبكرا.

الاقتراح: استخدام `React.lazy` و`Suspense` للصفحات الكبيرة، خصوصا صفحات الداشبورد. يوجد نمط جيد بالفعل في `src/pages/dashboard/InvoiceDetails.tsx:84-86` حيث يتم تحميل `html2canvas` و`jspdf` ديناميكيا.

### 13. Type safety ضعيف حاليا

`tsconfig.app.json:19-23` يعطل `strict`, `noUnusedLocals`, `noUnusedParameters`, `noImplicitAny`, و`noFallthroughCasesInSwitch`.

كذلك ESLint يعطل `@typescript-eslint/no-unused-vars` في `eslint.config.js:23`.

يوجد `any` صريح في أماكن مثل:

- `src/lib/reports.ts:8-11`
- `src/components/dashboard/CashFlowProjection.tsx:11-12`
- `src/components/dashboard/CashFlowProjection.tsx:170`
- `src/components/dashboard/DashboardLayout.tsx:198` و`217`

الأثر: اختلافات DTO، dead code، وأخطاء typing تصبح أسهل في الإفلات.

الاقتراح: تفعيل الصرامة تدريجيا، بدءا من ملفات DTO والـ API helpers المشتركة.

### 14. الاختبارات مجرد placeholder

الاختبار الوحيد هو `src/test/example.test.ts` ويختبر أن `true === true`.

الأثر: لا توجد حماية حقيقية لتدفقات auth، الفواتير، التدفق النقدي، التسجيل، أو الداشبورد.

الاقتراح: إضافة اختبارات مركزة لـ auth session handling، API unwrap/error handling، cashflow query params، registration verification، وإجراءات الفواتير الأساسية.

### 15. طبقة API مكررة بين الصفحات

عدة ملفات تعرف `API_BASE_URL`، endpoints، token retrieval، response parsing، ومعالجة أخطاء بشكل منفصل. أمثلة: `src/pages/Dashboard.tsx`، `src/pages/dashboard/Customers.tsx`، `src/pages/dashboard/Projects.tsx`، `src/pages/dashboard/Payments.tsx`، `src/pages/dashboard/Pricing.tsx`، و`src/components/dashboard/InvoicesTable.tsx`.

الأثر: معالجة الأخطاء غير موحدة، وتكرار auth logic، واحتمال drift بين الفرونت والباك إند.

الاقتراح: بناء طبقة API مشتركة typed، أو توليد client من عقود الباك إند.

## ترتيب إصلاح مقترح

1. إصلاح تنقل الموبايل وإتاحة إجراءات الحساب.
2. توحيد/تأمين تدفق Client Risk AI.
3. إصلاح cashflow parameters وإزالة الطلبات المكررة.
4. بناء Inbox للتنبيهات.
5. حذف routes المكررة والصفحة غير الموصولة.
6. ربط أو تعطيل نموذج التواصل.
7. إضافة lazy loading للصفحات الكبيرة وتقليل حجم bundle.
8. إضافة اختبارات حقيقية لمسارات auth، cashflow، registration، invoices.
