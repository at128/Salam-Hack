import { Link } from "react-router-dom";
import { ArrowUpLeft, Mail } from "lucide-react";

const productLinks = [
  { href: "#features", label: "المميزات" },
  { href: "#how", label: "كيف يعمل" },
  { href: "#pricing", label: "الأسعار" },
  { href: "#testimonials", label: "آراء العملاء" },
];

const companyLinks = [
  { href: "#", label: "من نحن" },
  { href: "#", label: "المدونة" },
  { href: "#contact", label: "تواصل معنا" },
];

const legalLinks = [
  { href: "#", label: "سياسة الخصوصية" },
  { href: "#", label: "شروط الاستخدام" },
];

export default function Footer() {
  return (
    <footer className="relative overflow-hidden bg-card text-navy border-t border-border">
      <div
        className="hidden"
        aria-hidden="true"
        style={{
          background:
            "radial-gradient(circle at 88% 18%, rgba(20, 184, 179, 0.18), transparent 22%), radial-gradient(circle at 12% 82%, rgba(14, 165, 160, 0.12), transparent 28%)",
        }}
      />

      <div className="container relative z-10 py-12 lg:py-14">
        <div className="grid gap-10 border-b border-border pb-8 md:grid-cols-2 lg:grid-cols-[1.2fr_repeat(3,minmax(0,1fr))] lg:gap-8">
          <div className="max-w-sm">
            <Link to="/" className="inline-flex items-center gap-3">
              <div className="grid h-10 w-10 place-items-center rounded-lg bg-navy text-base font-bold text-white shadow-glow">
                م
              </div>
              <div>
                <div className="text-lg font-bold text-navy">مالي</div>
                <div className="text-xs text-muted-foreground">إدارة مالية ذكية للمستقل العربي</div>
              </div>
            </Link>

            <p className="mt-4 text-sm leading-7 text-muted-foreground">
              منصة تساعدك على تنظيم الفواتير والمصاريف وقراءة أرباحك بصورة واضحة وسريعة.
            </p>

            <div className="mt-5 flex items-center gap-2 text-sm text-muted-foreground">
              <Mail className="h-4 w-4 text-teal" />
              hello@mali.app
            </div>
          </div>

          <div className="md:justify-self-end lg:justify-self-auto">
            <h4 className="text-sm font-semibold text-navy">المنتج</h4>
            <ul className="mt-4 space-y-3 text-sm text-muted-foreground">
              {productLinks.map((link) => (
                <li key={link.label}>
                  <a href={link.href} className="transition-colors hover:text-navy">
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          <div className="md:justify-self-start">
            <h4 className="text-sm font-semibold text-navy">الشركة</h4>
            <ul className="mt-4 space-y-3 text-sm text-muted-foreground">
              {companyLinks.map((link) => (
                <li key={link.label}>
                  <a href={link.href} className="transition-colors hover:text-navy">
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          <div className="md:col-span-2 lg:col-span-1 md:max-w-sm lg:max-w-none">
            <h4 className="text-sm font-semibold text-navy">قانوني</h4>
            <ul className="mt-4 space-y-3 text-sm text-muted-foreground">
              {legalLinks.map((link) => (
                <li key={link.label}>
                  <a href={link.href} className="transition-colors hover:text-navy">
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>

            <Link
              to="/register"
              className="mt-6 inline-flex items-center gap-2 text-sm font-semibold text-teal transition-colors hover:text-navy"
            >
              ابدأ استخدام مالي
              <ArrowUpLeft className="h-4 w-4" />
            </Link>
          </div>
        </div>

        <div className="flex flex-col gap-3 pt-5 text-xs text-muted-foreground md:flex-row md:items-center md:justify-between">
          <p>© ٢٠٢٦ مالي. جميع الحقوق محفوظة.</p>
          <div className="flex items-center gap-4">
            <a href="#pricing" className="transition-colors hover:text-navy">
              الأسعار
            </a>
            <a href="#features" className="transition-colors hover:text-navy">
              المميزات
            </a>
            <a href="#how" className="transition-colors hover:text-navy">
              كيف يعمل
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
}
