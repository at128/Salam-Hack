# تقرير مراجعة الفرونت اند — Salam-Hack

> تاريخ التقرير: 2026-05-01
> النطاق: `src/SalamHack.Client/src`
> نتيجة السحب: `git pull origin master` → `Already up to date`

## ملخص تنفيذي

تطبيق الويب (React + Vite + TypeScript + Tailwind + shadcn/ui) في حالة عمل جيدة ويغطي معظم ميزات الواجهة الأمامية المرتبطة بالعمليات الأساسية (العملاء، الفواتير، المشاريع، المصاريف، التحليلات). لكن هناك **5 مشاكل حرجة** تحتاج إلى معالجة عاجلة:

1. **تكرار المسارات الموجهة** في التطبيق (routes مرسلة إلى صفحات مختلفة بنفس path)
2. **عدم استخدام React Query** رغم تثبيته — كل الطلبات هي fetch مباشرة، مما يعني لا يوجد caching، invalidation، أو إدارة حالة
3. **عدم وجود متغيرات البيئة الأمامية موثقة** — لا يوجد `.env.example` للعميل، و `VITE_API_BASE_URL` الوحيد المستخدم
4. **SignalR معطل جزئياً** — تم إعداده لكن notification hub غير مرتبط بصفحة حقيقية للعرض
5. **معظم الاختبارات غائبة** — اختبار واحد فقط (مثال صوري)

---

## 1. مشاكل حرجة (Critical)

### [App.tsx:95-100](src/SalamHack.Client/src/App.tsx#L95)
**تكرار المسارات الموجهة (Duplicate Route Redirection)**

الأسطر 87-94 تعرّف المسارات الفعلية (`/dashboard/payments`، `/dashboard/pricing`، إلخ)، لكن الأسطر 95-100 تعيد تعريفها مع Navigate redirect. هذا يعني **المسارات الأصلية لن تكون أبداً مرئية**. الطريق الثاني سيظهر أولاً في قائمة الطرق.

```tsx
<Route path="payments" element={<PaymentsPage />} />                    // ← لن يتم الوصول إليها أبداً
<Route path="payments" element={<Navigate to="/dashboard/invoices" />} /> // ← الطريق الثاني ينتصر
```

**الحل**: احذف التعاريف المكررة (الأسطر 95-100)، أو غيّر المسارات الأصلية لتطابق الغرض الفعلي.

---

### [tsconfig.json + tsconfig.app.json](src/SalamHack.Client/tsconfig.json)
**إعدادات TypeScript ضعيفة جداً**

```json
{
  "compilerOptions": {
    "noImplicitAny": false,
    "noUnusedLocals": false,
    "noUnusedParameters": false,
    "strictNullChecks": false,
    "strict": false
  }
}
```

**المشاكل**:
- `strict: false` و `noImplicitAny: false` يسمحان بأي أنواع (any) بدون تحذير
- `noUnusedLocals` و `noUnusedParameters` معطلة — لن تحصل على تنبيهات عن الكود الميت
- `strictNullChecks: false` = null/undefined يمكن أن يُمرر في أي مكان دون اكتشاف

**التأثير**: أخطاء runtime محتملة وصعوبة الصيانة.

**الحل**: فعّل على الأقل:
```json
{
  "strict": true,
  "noImplicitAny": true,
  "strictNullChecks": true,
  "noUnusedLocals": true,
  "noUnusedParameters": true
}
```

---

### [DashboardLayout.tsx:198, 217](src/SalamHack.Client/src/components/dashboard/DashboardLayout.tsx#L198)
**نوع `any` للعمل مع SignalR**

```tsx
let connection: any = null;
connection.on("ReceiveNotification", (notification: any) => {
  if (!active) return;
  setHasNewNotifications(true);
});
```

- لا يوجد type safety لـ SignalR connection
- notification تُتجاهل تماماً (لا تُخزن أو تُعرض)
- لا طريقة لمعرفة ماذا يحتوي الإخطار

**الحل**: اكتب صيغة محكمة للـ SignalR types أو استخدم مكتبة typed-signalr.

---

### [CashFlowProjection.tsx, Breakdown.tsx, lib/reports.ts](src/SalamHack.Client/src/components/dashboard/CashFlowProjection.tsx)
**استخدام مفرط للـ `any` في التقارير**

```tsx
const [projection, setProjection] = useState<any>(null);
const [delayScenario, setDelayScenario] = useState<any>(null);
function delayBalance(delayScenario: any) { ... }
```

و في `lib/reports.ts`:
```tsx
byCustomer: any[];
byProject: any[];
topPerformers: any[];
lowestPerformers: any[];
```

**الحل**: عرّف interfaces واضحة (مثل `ProjectionData`, `ReportMetrics`).

---

### [استخدام localStorage للتخزين الحساس](src/SalamHack.Client/src/lib/auth.ts#L44)
**المشكلة الأمنية**: التوكن يُحفظ في localStorage أو sessionStorage بدون encryption.

```tsx
const storage = rememberMe ? localStorage : sessionStorage;
storage.setItem("accessToken", result.token.accessToken);
```

**الخطورة**:
- أي JavaScript في الصفحة (XSS) يمكنه الوصول إلى التوكن
- التخزين في localStorage ينتج حماية **0** ضد XSS

**الحل**: استخدم httpOnly cookies بدلاً من localStorage (يتطلب تغييراً من الخادم).

---

## 2. ميزات ناقصة (Missing Features)

### صفحة الإخطارات (Notifications Page)
يوجد SignalR hub معد (`/hubs/notifications`) وزر جرس في الـ header يُعدّ `hasNewNotifications` إلى false عند النقر، **لكن لا توجد صفحة فعلية لعرض الإخطارات**.

**الحل**: أنشئ `pages/dashboard/Notifications.tsx` واربطها بـ route `/dashboard/notifications` وقم بجلب الإخطارات من API endpoint.

---

### صفحة التقارير المتقدمة (Advanced Reports Page)
يوجد `lib/reports.ts` مع `ProfitabilityReportDto` و `fetchProfitabilityReport()`، لكن **لا توجد صفحة Reports موحدة**. المستخدم يرى الأرباح فقط في `Profit.tsx` و `Breakdown.tsx`.

**الحل**: أنشئ `pages/dashboard/Reports.tsx` تجمع جميع التقارير (الربح، التحليلات، الإيرادات حسب العميل/المشروع، إلخ).

---

### تصدير الفواتير (Invoice Export Beyond Print)
الفواتير توفر طباعة فقط، بدون تصدير CSV أو PDF binary.

**الحل**: أضف أزرار تصدير CSV/PDF من `pages/dashboard/Invoices.tsx`.

---

### وظائف البحث والتصفية المتقدمة
صفحات مثل Customers و Invoices لديها search بسيط، لكن **بدون filters متقدمة** (نطاق التاريخ، الحالة، المبلغ، إلخ).

**الحل**: أضف فلاتر متقدمة باستخدام معاملات query API.

---

### خاصية الفرز (Sorting)
لا توجد ميزة فرز في الجداول — بدون نقر على رؤوس الأعمدة للفرز صعوداً/هبوطاً.

**الحل**: أضف معاملات `sortBy` و `sortDirection` إلى جلب البيانات.

---

### نافذة تأكيد الحذف قبل الإجراء المدمر
مثل: حذف فاتورة، عميل، مشروع — يستخدم Swal أو Dialog، **لكن بدون تنبيه تحذيري واضح** (مثل "لا يمكن التراجع").

معظم الأماكن بها تأكيد، لكن بعض الأماكن قد تفتقد (مثل حذف المشاريع بدون تحذير من الفواتير المرتبطة).

---

## 3. مشاكل في الـ API Integration

### [عدم استخدام React Query](src/SalamHack.Client/src/App.tsx#L2)

**المشكلة**: React Query مثبتة (`@tanstack/react-query` في package.json)، **لكن لا تُستخدم أبداً**. كل الصفحات تستخدم `fetch` مباشرة مع `useEffect` و `useState`.

```tsx
// ❌ الطريقة الحالية
const [customers, setCustomers] = useState(null);
useEffect(() => {
  customersRequest().then(setCustomers).catch(setError);
}, []);

// ✅ الطريقة الصحيحة مع React Query
const { data: customers, isLoading, error } = useQuery({
  queryKey: ['customers', filters],
  queryFn: () => customersRequest()
});
```

**المشاكل التي تنتج**:
- **لا caching**: إعادة حمل الصفحة = اتصال API جديد دائماً
- **لا invalidation**: تحديث عميل واحد لا يُحدّث الجدول
- **N+1 fetches**: تحميل 10 عملاء = 10 طلبات منفصلة (بدلاً من طلب واحد paginated)
- **لا background refetch**: البيانات القديمة لا تُنعش تلقائياً

**الحل**: أعِد بناء جميع `useEffect` + `fetch` patterns ليستخدموا `useQuery` و `useMutation`.

---

### [عدم وجود Global Error Handler](src/SalamHack.Client/src/lib/auth.ts#L162)

كل endpoint يتعامل مع الأخطاء منفصلة. توجد دالة `getApiErrorMessage()`، لكن **لا توجد interceptor أو middleware وسطي** لمعالجة:
- 401 (token منتهي الصلاحية) — يُعاد التوجيه إلى login في بعض الأماكن، لا في غيرها
- 403 (ممنوع) — بدون رسالة واضحة
- 500 (خطأ الخادم) — بدون retry automation
- Network timeout — بدون timeout معرّف

**الحل**: أنشئ `lib/api-client.ts` مع axios أو fetch wrapper يتعامل مع:
- حقن Authorization header تلقائياً
- تحديث التوكن عند 401
- إعادة محاولة exponential backoff على 429/503
- تسجيل أخطاء مركزي

---

### [عدم وجود Error Boundaries](src/SalamHack.Client/src/pages/dashboard/Customers.tsx)

معظم الصفحات لديها `isLoading` و `error` states، **لكن بدون boundary component عام** للتقاط أخطاء الصفحة الرئيسية.

مثال: إذا فشل جلب الملف الشخصي في DashboardLayout، **الصفحة تنهار بدون رسالة**.

**الحل**: أنشئ `components/ErrorBoundary.tsx` و `components/FallbackError.tsx`.

---

### [عدم وجود Request Timeout محدّد](src/SalamHack.Client/src/lib/auth.ts#L104)

جميع الـ fetch calls بدون timeout. إذا الخادم تجمد، **سينتظر المستخدم للأبد**.

**الحل**: أضف AbortController مع timeout:
```tsx
const controller = new AbortController();
const timeoutId = setTimeout(() => controller.abort(), 10000); // 10 ثوان
const response = await fetch(url, { signal: controller.signal });
```

---

## 4. مشاكل في الـ Routing والـ Navigation

### [Routes مسدودة (Unreachable Routes)](src/SalamHack.Client/src/App.tsx#L95)

كما ذُكر في القسم 1:

```tsx
<Route path="payments" element={<PaymentsPage />} />               // ← لن تُصل إليها
<Route path="payments" element={<Navigate to="..." />} />          // ← هذه تحجبها
```

**الحل**: احذف التعاريف المكررة.

---

### [عدم وجود 404 Page للـ Sub-Routes](src/SalamHack.Client/src/App.tsx#L79)

صفحة NotFound موجودة، لكن **فقط للـ top-level routes**. إذا ذهب المستخدم إلى `/dashboard/foo-invalid`، سيرى محتوى صفحة Dashboard الفارغة.

**الحل**: أضف fallback route داخل `DashboardLayout`:
```tsx
<Route path="*" element={<NotFound />} />
```

---

### [عدم وجود Lazy Loading للـ Routes](src/SalamHack.Client/src/App.tsx#L7)

جميع الصفحات مستوردة مباشرة (eager import). هذا يعني **bundle الأول يحتوي على كود جميع الصفحات**، حتى لو لم يزر المستخدم 90% منها.

**الحل**: استخدم React.lazy():
```tsx
const Dashboard = lazy(() => import('./pages/Dashboard'));
const Invoices = lazy(() => import('./pages/dashboard/Invoices'));
// أضف <Suspense> wrapper
```

---

### [Return URL غير محفوظ بأمان في Login](src/SalamHack.Client/src/pages/Login.tsx#L66)

```tsx
navigate(searchParams.get("returnUrl") || "/dashboard", { replace: true });
```

**المشكلة**: لا يوجد validation على `returnUrl`. المستخدم يمكنه تمرير `?returnUrl=https://evil.com`.

**الحل**: تحقق من أن الـ URL محلي:
```tsx
const returnUrl = searchParams.get("returnUrl");
const isLocalUrl = returnUrl?.startsWith("/") && !returnUrl?.startsWith("//");
navigate(isLocalUrl ? returnUrl : "/dashboard", { replace: true });
```

---

## 5. مشاكل في الـ Forms والـ Validation

### [عدم استخدام React Hook Form بشكل موحد](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L295)

كل form تستخدم `useState` مباشرة:

```tsx
const [form, setForm] = useState<CustomerForm>(EMPTY_FORM);
const setField = (field: keyof CustomerForm) => (event) => {
  setForm((prev) => ({ ...prev, [field]: event.target.value }));
};
```

React Hook Form مثبتة ولكن **لا تُستخدم**. هذا يعني:
- تكرار الكود
- بدون debouncing
- بدون validation real-time
- بدون form state tracking محترف

**الحل**: أعِد بناء `Customers.tsx`, `Projects.tsx`, `Expenses.tsx` لاستخدام:
```tsx
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

const schema = z.object({
  customerName: z.string().min(1, "الاسم مطلوب"),
  email: z.string().email("بريد غير صحيح"),
});

const form = useForm({ resolver: zodResolver(schema), defaultValues: {...} });
```

---

### [معالجة الأخطاء غير المتسقة](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L107)

بعض الصفحات لديها `normalizeValidationErrors()`، وبعضها تستدعي `getErrorText()` مباشرة. لا توجد معيارية.

**الحل**: أنشئ `hooks/useFormErrors.ts` توحد المعالجة.

---

### [عدم وجود Client-Side Validation قبل الإرسال](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L306)

Form يسمح بالإرسال حتى لو كانت الحقول فارغة (على الرغم من `required` attribute HTML)، لكن **بدون validation Zod** قبل الطلب API.

**الحل**: استخدم Zod schema validation (انظر أعلى).

---

## 6. مشاكل في State Management / React Query

### [عدم استخدام React Query على الإطلاق](src/SalamHack.Client/src/App.tsx#L37)

```tsx
const queryClient = new QueryClient();
// ... لكن لا يُستخدم أبداً في أي صفحة
```

**الخطوط الزمنية**:
- **قصيرة**: احذف من `App.tsx` و `package.json`
- **طويلة**: أعِد بناء جميع `useEffect` fetches → `useQuery`

---

### [عدم وجود Global State للمستخدم](src/SalamHack.Client/src/lib/auth.ts#L119)

حالة المستخدم محفوظة في localStorage + window events:

```tsx
export function getCurrentUser(): AuthUser | null {
  const raw = localStorage.getItem("currentUser") ?? sessionStorage.getItem("currentUser");
  return JSON.parse(raw);
}
window.dispatchEvent(new Event("auth:user-updated"));
```

هذا يعمل، **لكن غير موثوق في الصفحات المتعددة**. سينكرونيس فقط عند تحديث localStorage في نفس التبويب.

**الحل**: استخدم Context API أو Zustand للحالة العامة.

---

### [عدم وجود Optimistic Updates](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L322)

عند حذف عميل:

```tsx
await customersRequest(`/${deleteTarget.id}`, { method: "DELETE" });
setMessage("تم حذف العميل بنجاح.");
await loadCustomers(); // ← إعادة تحميل كاملة
```

**الأفضل**: تحديث الـ local state قبل الطلب، ثم rollback إذا فشل:

```tsx
const oldCustomers = customers;
setCustomers(customers.filter(c => c.id !== deleteTarget.id)); // optimistic
try {
  await customersRequest(...);
} catch {
  setCustomers(oldCustomers); // rollback
}
```

---

## 7. مشاكل في الـ i18n / RTL / الترجمة

### [RTL معد بشكل صحيح](src/SalamHack.Client/index.html#L2)

```html
<html lang="ar" dir="rtl">
```

و `tailwind.config.ts` يحتوي على theme محدد للعربية. **هذا جيد**.

---

### [الترجمة مخلوطة (Mixed Arabic/English)](src/SalamHack.Client/src/pages/Login.tsx#L60)

معظم النصوص بالعربية بشكل صحيح، لكن:
- Login page: "تعذر تسجيل الدخول" + "تعذر الاتصال بالخدمة" (عربي جيد)
- Dashboard: "ابدأ بمشروعك الأول" (عربي جيد)
- لكن بعض الصفحات تحتوي على placeholder/label إنجليزية:

```tsx
placeholder="name@example.com"  // ← should be localized
```

**الحل**: أنشئ `lib/i18n.ts` أو استخدم `react-i18next` (إذا كنت تريد multi-language لاحقاً).

---

### [يوميات الأشهر مثبتة بالكود](src/SalamHack.Client/src/pages/dashboard/Profit.tsx#L35)

```tsx
const months = ["يناير", "فبراير", "مارس", ...];
return months[t.month - 1] + " " + String(t.year).slice(-2);
```

تمام (عربي)، لكن بدون localization. إذا أضفت English لاحقاً، ستحتاج إلى تغيير يدوي.

---

## 8. مشاكل في الـ Auth Flow

### [معالجة انتهاء صلاحية التوكن منتشرة](src/SalamHack.Client/src/lib/auth.ts#L180)

دالة `getValidAccessToken()` موجودة وتتعامل مع التحديث، **لكن التحديث لا يُستدعى في جميع الأماكن**.

مثال: `Dashboard.tsx` استدعاء `getValidAccessToken()` مباشرة (سطر 105):

```tsx
const token = await getValidAccessToken();
```

لكن بعض الأماكن في `Customers.tsx` تُستدعي `customersRequest()` التي تستدعيها بداخلها (سطر 85):

```tsx
async function customersRequest<T>(path = "", init?: RequestInit): Promise<T> {
  const token = await getValidAccessToken(); // يُتعامل مع التحديث
```

**النتيجة**: غير متسق. بعض الصفحات آمنة من انتهاء الصلاحية، وبعضها قد لا تكون.

**الحل**: اجعل API middleware مركزي يتعامل مع التحديث دائماً.

---

### [Refresh Token بدون Retry](src/SalamHack.Client/src/lib/auth.ts#L153)

عند استدعاء `/Auth/refresh`، إذا فشل:

```tsx
if (!response.ok) {
  clearAuthSession();
  return false;
}
```

لا إعادة محاولة. إذا كان هناك مشكلة شبكة مؤقتة، سيتم حذف الجلسة فوراً.

**الحل**: أضف retry logic مع exponential backoff.

---

### [عدم وجود تسجيل الخروج من جميع الأجهزة](src/SalamHack.Client/src/lib/auth.ts#L101)

Logout يُمسح فقط localStorage/sessionStorage محلياً:

```tsx
export function clearAuthSession() {
  clearAuthStorage(localStorage);
  clearAuthStorage(sessionStorage);
}
```

لا طلب API يُخبر الخادم بـ logout. إذا كان المستخدم logged in على جهازين، الجهاز الثاني سيبقى logged in.

**الحل**: أضف طلب `/Auth/logout` POST إلى الخادم.

---

### [عدم وجود Two-Factor Authentication](src/SalamHack.Client/src/pages/RegisterVerify.tsx)

تحقق OTP موجود للتسجيل الجديد، لكن بدون 2FA لـ login الموجود.

**إذا كان الخادم يدعمه**: أضف صفحة 2FA بعد login.

---

## 9. مشاكل UX / تجربة المستخدم

### [عدم وجود Loading Skeletons](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L451)

Loading state يعرض نص فقط:

```tsx
{isLoading ? (
  <div className="flex items-center justify-center...">
    <Loader2 className="ml-2 h-4 w-4 animate-spin" />
    جاري تحميل العملاء...
  </div>
) : ...}
```

**الأفضل**: Skeleton loaders تحاكي شكل الجدول:

```tsx
{isLoading ? (
  <div className="space-y-2">
    <Skeleton className="h-12 w-full" />
    <Skeleton className="h-12 w-full" />
  </div>
) : ...}
```

---

### [عدم وجود Empty States واضحة](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L551)

صفحة بدون عملاء تعرض:

```tsx
<Users className="mb-3 h-8 w-8 text-muted-foreground" />
<p className="font-semibold text-navy">لا توجد عملاء للعرض</p>
```

جيد، لكن بدون CTA واضحة:

```tsx
<Button onClick={openCreateForm} className="mt-4">إنشاء أول عميل</Button>
```

---

### [عدم وجود Pagination UI واضحة](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L524)

الـ pagination موجودة في الكود (Previous/Next)، لكن بدون عرض عدد الصفحات الإجمالي أو شريط progress:

```tsx
<span>صفحة {customers.pageNumber} من {customers.totalPages || 1}</span>
```

**الأفضل**: أضف رقم الصفحات الحالية (٢/٥) وعدد العناصر الإجمالي.

---

### [عدم وجود toast/notification على الإجراءات الناجحة](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L328)

عند إنشاء عميل بنجاح:

```tsx
setMessage("تم إنشاء العميل بنجاح.");
setIsFormOpen(false);
```

مقبول، لكن بدون toast notification (من `sonner` أو `shadcn/ui/toaster`).

```tsx
import { toast } from "sonner";
toast.success("تم إنشاء العميل بنجاح!");
```

---

### [عدم وجود Confirmation Dialogs على الحذف المدمر](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L736)

تأكيد موجود (Dialog)، لكن بدون تحذير واضح:

```tsx
<DialogDescription>
  هل تريد حذف {deleteTarget?.customerName}؟ لا يمكن التراجع عن هذا الإجراء.
</DialogDescription>
```

**الأفضل**: أضف icon warning + شريط danger حمراء.

---

## 10. مشاكل في جودة الكود

### [استخدام مفرط للـ `any` types](src/SalamHack.Client/src/components/dashboard/CashFlowProjection.tsx#L5)

```tsx
const [projection, setProjection] = useState<any>(null);
const [delayScenario, setDelayScenario] = useState<any>(null);
function delayBalance(delayScenario: any) { }
```

و 8+ أماكن أخرى.

---

### [استيراد غير مستخدم](src/SalamHack.Client/src/pages/dashboard/Profit.tsx)

ESLint معطل (`noUnusedLocals: false`)، لذا لا تنبيهات.

**الحل**: فعّل `noUnusedLocals` في tsconfig.

---

### [عدم وجود Constants منفصلة](src/SalamHack.Client/src/pages/dashboard/Invoices.tsx#L90)

API URLs معرّفة في كل صفحة:

```tsx
const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const INVOICES_API_URL = `${API_BASE_URL}/api/v1/invoices`;
```

تكرار في 15+ صفحة. **عدم DRY (Don't Repeat Yourself)**.

**الحل**: أنشئ `lib/api-constants.ts`:
```tsx
export const CUSTOMERS_URL = `${API_BASE_URL}/api/v1/customers`;
export const INVOICES_URL = `${API_BASE_URL}/api/v1/invoices`;
```

---

### [عدم وجود Error Logging / Monitoring](src/SalamHack.Client/src/components/dashboard/DashboardLayout.tsx#L225)

```tsx
} catch (err) {
  console.warn("SignalR Connection failed: ", err);
}
```

فقط console.warn — لا error tracking service (مثل Sentry أو LogRocket).

---

## 11. مشاكل البناء والإعدادات

### [عدم وجود .env.example للعميل](src/SalamHack.Client)

الجذر يحتوي على `.env.example`، لكن بدون متغيرات frontend. الـ `VITE_API_BASE_URL` الوحيد المستخدم غير موثق.

**الحل**: أنشئ `src/SalamHack.Client/.env.example`:
```
VITE_API_BASE_URL=http://localhost:5010
VITE_LOG_LEVEL=info
VITE_ENABLE_SIGNALR=true
```

---

### [Build Mode بدون Optimizations واضحة](src/SalamHack.Client/vite.config.ts)

```tsx
export default defineConfig(({ mode }) => ({
  // ... بدون build optimizations محددة
}));
```

**الأفضل**: أضف:
```tsx
build: {
  rollupOptions: {
    output: {
      manualChunks: {
        'vendor': ['react', 'react-dom'],
        'ui': ['@radix-ui/*'],
        'charts': ['recharts']
      }
    }
  },
  sourcemap: mode === 'development'
}
```

---

### [ESLint Warnings معطلة](src/SalamHack.Client/eslint.config.js#L23)

```tsx
"@typescript-eslint/no-unused-vars": "off",
```

تحذيرات متغيرات غير مستخدمة معطلة. بدون feedback عن الكود الميت.

**الحل**: شغّل على الأقل كـ warn:
```tsx
"@typescript-eslint/no-unused-vars": ["warn"],
```

---

## 12. الاختبارات

### [عدم وجود Tests فعلية](src/SalamHack.Client/src/test/example.test.ts)

```tsx
describe("example", () => {
  it("should pass", () => {
    expect(true).toBe(true);
  });
});
```

تغطية اختبار = 0%. Vitest معد في `package.json`، لكن بدون اختبارات حقيقية.

**الحل**: أضف:
1. Unit Tests للـ Auth Logic (token refresh, expiry checks)
2. Component Tests للنماذج الرئيسية
3. Integration Tests للـ Forms (مع MSW mock API)

---

## 13. الـ Accessibility

### [Aria Labels ناقصة في بعض الأماكن](src/SalamHack.Client/src/pages/Login.tsx#L137)

```tsx
<button type="button" onClick={() => setShowPwd((s) => !s)} aria-label="إظهار كلمة المرور">
```

جيد. لكن بعض الأيقونات الأخرى بدون aria-label:

```tsx
<Bell className="h-4 w-4" /> {/* ← بدون aria-label */}
```

**الحل**: أضف لجميع الأيقونات:
```tsx
<Bell className="h-4 w-4" aria-label="الإخطارات" />
```

---

### [Form Labels متصلة بـ Inputs](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L575)

```tsx
<Label htmlFor="customerName" required>اسم العميل</Label>
<Input id="customerName" ... />
```

ممتاز.

---

### [Color Contrast في الـ Dark Mode](src/SalamHack.Client/src/pages/dashboard/Customers.tsx)

Tailwind colors معرفة بـ CSS variables. لم أجد تناقض واضح، لكن الاختبار يحتاج إلى manual verification.

**الحل**: استخدم online tool مثل WCAG Contrast Checker.

---

## 14. الأمان

### [localStorage بدون Encryption](src/SalamHack.Client/src/lib/auth.ts#L75)

```tsx
storage.setItem("accessToken", result.token.accessToken);
```

XSS attack يمكنه الوصول:
```tsx
localStorage.getItem("accessToken")
```

**الحل**: استخدم httpOnly cookies من الخادم.

---

### [عدم وجود CSRF Protection](src/SalamHack.Client/src/pages/Login.tsx#L42)

جميع الطلبات POST بدون CSRF token.

**إذا كان الخادم يتطلب CSRF**: أضف header:
```tsx
headers: {
  "X-CSRF-Token": getCsrfToken()
}
```

---

### [عدم وجود Content Security Policy (CSP)](src/SalamHack.Client/index.html)

بدون CSP header.

**الحل**: أضف إلى nginx.conf:
```nginx
add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline'";
```

---

### [عدم وجود X-Frame-Options](src/SalamHack.Client/index.html)

بدون حماية من clickjacking.

**الحل**: nginx header:
```nginx
add_header X-Frame-Options "DENY";
```

---

## 15. الأداء

### [عدم وجود Code Splitting للـ Routes](src/SalamHack.Client/src/App.tsx#L7)

جميع الصفحات محملة eagerly:

```tsx
import Dashboard from "./pages/Dashboard.tsx";
import InvoicesPage from "./pages/dashboard/Invoices.tsx";
// ... 15 pages أخرى
```

Bundle الأول كبير جداً.

**الحل**: استخدم `React.lazy()` + `<Suspense>`.

---

### [عدم وجود Image Optimization](src/SalamHack.Client/src/components/landing/Hero.tsx)

إذا كانت هناك صور في Landing page، بدون lazy loading.

**الحل**: استخدم `<img loading="lazy" />`.

---

### [عدم وجود Caching Headers](src/SalamHack.Client/nginx.conf)

ربما nginx معد، لكن بدون Cache-Control headers:

```nginx
expires 30d;
add_header Cache-Control "public, immutable";
```

---

### [عدم وجود Virtual Scrolling للجداول الكبيرة](src/SalamHack.Client/src/pages/dashboard/Customers.tsx#L457)

جدول يرسم جميع الصفوف في DOM حتى لو كان الجدول يحتوي على 1000 صف:

```tsx
{customers.items.map((customer) => (
  <tr key={customer.id}>...</tr>
))}
```

**الحل**: استخدم `react-window` أو `tanstack/react-virtual`.

---

## أولوية الإصلاح المقترحة

### الأولوية 1 (Critical — إصلح الأسبوع الأول)

1. **[App.tsx:95-100]** — احذف التعاريف المسدودة للـ routes المكررة
2. **[tsconfig.json]** — فعّل `strict: true` و `noImplicitAny: true`
3. **[استخدام React Query]** — استثمر 2-3 أيام لتحويل جميع `useEffect` → `useQuery`
4. **[auth.ts:75]** — استخدم httpOnly cookies بدلاً من localStorage
5. **[API Middleware]** — أنشئ `lib/api-client.ts` مركزي مع retry/timeout logic

---

### الأولوية 2 (High — الأسابيع 2-3)

6. **[استبدال useState Forms]** — استخدم React Hook Form + Zod في جميع الصفحات
7. **[Error Boundary]** — أنشئ global error handling
8. **[SignalR + Notifications Page]** — أكمل الإخطارات بصفحة فعلية
9. **[Code Splitting]** — لزيّ الـ routes مع `React.lazy()`
10. **[.env.example]** — وثّق متغيرات البيئة

---

### الأولوية 3 (Medium — الأسابيع 4-5)

11. **[الاختبارات]** — ابدأ مع unit tests للـ auth + components
12. **[Performance]** — أضف loading skeletons + virtual scrolling للجداول الكبيرة
13. **[Accessibility]** — أضف aria-labels لجميع الأيقونات
14. **[Toast Notifications]** — استبدل `setMessage` مع `sonner` toasts
15. **[Keyboard Navigation]** — اختبر Tab navigation في جميع الـ dialogs

---

### الأولوية 4 (Nice-to-Have — المستقبل)

16. **[Advanced Reports Page]** — وحّد جميع التقارير في صفحة واحدة
17. **[Export Data]** — أضف CSV/PDF export للفواتير والعملاء
18. **[Dark Mode]** — اختبر و حسّن dark theme (next-themes معد)
19. **[i18n]** — قدّم دعم لغات متعددة (إذا كان مخططاً)
20. **[Logging Service]** — أضف Sentry أو LogRocket

---

## الملاحظات الختامية

**القوة**:
- RTL و Arabic UX محترفة
- UI جميل مع Tailwind و shadcn
- معظم الميزات الأساسية موجودة

**الضعف**:
- TypeScript loose (strict: false)
- State management بدائي (بدون React Query)
- Security risks (localStorage tokens)
- Duplicate routes & unreachable code
- اختبارات قليلة جداً

**الملخص**: التطبيق عملي وجاهز للإنتاج بشكل أساسي، لكن يحتاج إلى تحسينات هيكلية (React Query، TypeScript strict، API middleware) واختبارات شاملة قبل الإطلاق على المستخدمين الفعليين.
