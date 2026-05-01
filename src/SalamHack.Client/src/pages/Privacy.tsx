import { Link } from "react-router-dom";

export default function Privacy() {
  return (
    <main className="min-h-screen bg-background px-5 py-10" dir="rtl">
      <section className="mx-auto max-w-3xl rounded-2xl border border-border/70 bg-card p-6 shadow-card">
        <Link to="/" className="text-sm font-semibold text-teal hover:underline">
          مالي
        </Link>
        <h1 className="mt-4 text-3xl font-extrabold text-navy">سياسة الخصوصية</h1>
        <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
          توضح هذه السياسة كيف نتعامل مع بياناتك عند استخدام نظام مالي.
        </p>

        <div className="mt-6 space-y-5 text-sm leading-7 text-muted-foreground">
          <section>
            <h2 className="mb-1 text-lg font-bold text-navy">البيانات التي نجمعها</h2>
            <p>نجمع بيانات الحساب الأساسية والبيانات التي تدخلها داخل النظام مثل العملاء، المشاريع، الفواتير، والمصروفات.</p>
          </section>

          <section>
            <h2 className="mb-1 text-lg font-bold text-navy">استخدام البيانات</h2>
            <p>نستخدم البيانات لتشغيل الخدمة، إنشاء التقارير، تحسين تجربة الاستخدام، وتأمين الحساب.</p>
          </section>

          <section>
            <h2 className="mb-1 text-lg font-bold text-navy">حماية البيانات</h2>
            <p>نعتمد ضوابط فنية وتنظيمية لحماية البيانات، وننصحك بالحفاظ على سرية كلمة المرور وتحديثها عند الحاجة.</p>
          </section>
        </div>
      </section>
    </main>
  );
}
