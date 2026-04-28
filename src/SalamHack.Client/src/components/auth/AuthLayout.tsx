import { Link } from "react-router-dom";
import { ShieldCheck, Sparkles, TrendingUp } from "lucide-react";

type Props = {
  title: string;
  subtitle: string;
  children: React.ReactNode;
  footer: React.ReactNode;
  badge?: string;
};

export default function AuthLayout({ title, subtitle, children, footer, badge }: Props) {
  return (
    <div className="h-screen overflow-hidden grid lg:grid-cols-2 bg-background">
      {/* Form side */}
      <div className="flex flex-col justify-center px-6 sm:px-10 lg:px-16 py-4 lg:py-5">
        <Link to="/" className="flex items-center gap-2 mb-3 lg:mb-4">
          <div className="w-9 h-9 rounded-xl bg-gradient-brand text-white grid place-items-center font-bold shadow-glow">
            م
          </div>
          <span className="font-bold text-lg text-navy">مالي</span>
        </Link>

        <div className="w-full max-w-md mx-auto lg:mx-0">
          {badge && (
            <span className="inline-flex items-center gap-2 rounded-full bg-teal-soft text-teal px-3 py-1 text-xs font-semibold mb-2">
              <Sparkles className="w-3.5 h-3.5" />
              {badge}
            </span>
          )}
          <h1 className="text-2xl lg:text-[1.7rem] font-bold text-navy tracking-tight">{title}</h1>
          <p className="mt-1 text-sm text-muted-foreground leading-relaxed">{subtitle}</p>

          <div className="mt-4">{children}</div>

          <div className="mt-3 text-sm text-muted-foreground text-center">{footer}</div>
        </div>
      </div>

      {/* Visual side */}
      <div className="hidden lg:block relative overflow-hidden bg-gradient-hero">
        <div className="absolute inset-0 opacity-30"
          style={{
            backgroundImage:
              "radial-gradient(circle at 20% 20%, hsl(var(--teal) / 0.45), transparent 45%), radial-gradient(circle at 80% 70%, hsl(var(--teal-light) / 0.35), transparent 50%)",
          }}
        />

        <div className="relative h-full flex flex-col justify-between p-10 xl:p-12 text-white">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full bg-white/10 backdrop-blur px-3 py-1 text-xs font-semibold border border-white/15">
              <ShieldCheck className="w-3.5 h-3.5" />
              بياناتك مشفّرة وآمنة
            </span>
            <h2 className="mt-5 text-3xl xl:text-4xl font-bold leading-snug max-w-md">
              مالية أوضح، قرارات أذكى،
              <br />
              ودخل ينمو معك.
            </h2>
            <p className="mt-3 text-sm xl:text-base text-white/70 max-w-md leading-relaxed">
              انضم لأكثر من ١٢ ألف مستقل عربي يديرون فواتيرهم وأرباحهم من مكان واحد.
            </p>
          </div>

          {/* Floating preview cards */}
          <div className="relative h-72">
            <div className="absolute top-0 right-0 w-64 bg-white/95 text-navy rounded-2xl p-5 shadow-elevated animate-float">
              <div className="flex items-center justify-between">
                <span className="text-xs text-muted-foreground">إيراد هذا الشهر</span>
                <span className="text-xs font-bold text-success bg-success-soft px-2 py-0.5 rounded-full">
                  +19٪
                </span>
              </div>
              <div className="mt-2 text-2xl font-bold">28,000 ر.س</div>
              <div className="mt-4 flex items-end gap-1.5 h-14">
                {[40, 60, 45, 75, 55, 90, 70].map((h, i) => (
                  <div
                    key={i}
                    className="flex-1 rounded-t bg-gradient-brand"
                    style={{ height: `${h}%` }}
                  />
                ))}
              </div>
            </div>

            <div
              className="absolute bottom-0 left-0 w-60 bg-white/95 text-navy rounded-2xl p-4 shadow-elevated animate-float flex items-center gap-3"
              style={{ animationDelay: "1.5s" }}
            >
              <div className="w-11 h-11 rounded-xl bg-teal-soft text-teal grid place-items-center">
                <TrendingUp className="w-5 h-5" />
              </div>
              <div>
                <div className="text-xs text-muted-foreground">هامش الربح</div>
                <div className="text-base font-bold">٦٧٪</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
