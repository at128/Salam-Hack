import { Link } from "react-router-dom";

export default function Terms() {
  return (
    <main className="min-h-screen bg-background px-5 py-10" dir="rtl">
      <section className="mx-auto max-w-3xl rounded-2xl border border-border/70 bg-card p-6 shadow-card">
        <Link to="/" className="text-sm font-semibold text-teal hover:underline">
          مالي
        </Link>
        <h1 className="mt-4 text-3xl font-extrabold text-navy">شروط الاستخدام</h1>
        <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
          باستخدامك لنظام مالي فإنك توافق على استخدام الخدمة لإدارة أعمالك وبياناتك المالية بشكل نظامي ومسؤول.
        </p>

        <div className="mt-6 space-y-5 text-sm leading-7 text-muted-foreground">
          <section>
            <h2 className="mb-1 text-lg font-bold text-navy">استخدام الخدمة</h2>
            <p>يجب إدخال بيانات صحيحة وعدم استخدام المنصة لأي نشاط مخالف أو يضر بالخدمة أو المستخدمين الآخرين.</p>
          </section>

          <section>
            <h2 className="mb-1 text-lg font-bold text-navy">الحساب والأمان</h2>
            <p>أنت مسؤول عن سرية بيانات الدخول الخاصة بك وعن أي نشاط يتم من خلال حسابك.</p>
          </section>

          <section>
            <h2 className="mb-1 text-lg font-bold text-navy">البيانات المالية</h2>
            <p>التقارير والتحليلات داخل النظام أدوات مساعدة ولا تعتبر بديلاً عن الاستشارة المحاسبية أو القانونية المتخصصة.</p>
          </section>
        </div>
      </section>
    </main>
  );
}
