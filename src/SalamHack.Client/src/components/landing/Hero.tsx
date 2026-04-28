import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Sparkles, Star, ShieldCheck, Brain } from "lucide-react";
import heroImage from "@/assets/hero-illustration.png";

export default function Hero() {
  return (
    <section className="relative overflow-hidden border-b border-border">
      <div className="hero-wave" aria-hidden="true" />
      <div className="container relative z-10 grid lg:grid-cols-2 gap-12 items-center pt-8 pb-20 lg:pt-12 lg:pb-28">
        {/* Text */}
        <div className="animate-fade-up text-center lg:text-right">
          <span className="inline-flex items-center gap-2 rounded-lg border border-border bg-card text-teal px-4 py-1.5 text-xs font-semibold mb-6 shadow-glow">
            <Sparkles className="w-3.5 h-3.5" />
            مدعوم بالذكاء الاصطناعي
          </span>

          <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold text-navy leading-tight tracking-tight">
            سعّر صح، اعرف عميلك،
            <span className="block mt-3 text-teal">
              اعرف ربحك الحقيقي
            </span>
          </h1>

          <p className="mt-6 text-lg text-muted-foreground leading-relaxed max-w-xl mx-auto lg:mx-0">
            ثلاثة أسئلة يسألها كل مستقل ولا يجد إجابة واضحة — مالي يجيب عليها بالبيانات الفعلية، لا بالتخمين.
          </p>

          {/* 3 core problems callout */}
          <div className="mt-6 flex flex-col sm:flex-row gap-2 justify-center lg:justify-start text-xs">
            {[
              { label: "التسعير الذكي", color: "bg-teal-soft text-teal" },
              { label: "تحليل العميل", color: "bg-warning-soft text-navy" },
              { label: "محلل الأرباح الذكي", color: "bg-success-soft text-success" },
            ].map((p) => (
              <span key={p.label} className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-border bg-card text-navy font-semibold shadow-glow`}>
                <span className="w-1.5 h-1.5 bg-teal opacity-80" />
                {p.label}
              </span>
            ))}
          </div>

          <div className="mt-8 flex flex-col sm:flex-row gap-3 justify-center lg:justify-start">
            <Link to="/register">
              <Button
                size="lg"
                className="rounded-lg bg-navy hover:bg-navy-light shadow-glow px-8 h-12 text-base"
              >
                ابدأ مجاناً الآن
                <ArrowLeft className="w-4 h-4 mr-1" />
              </Button>
            </Link>
            <a href="#how">
              <Button
                size="lg"
                variant="outline"
                className="rounded-lg px-8 h-12 text-base border-navy/20 bg-card hover:bg-muted/70"
              >
                شاهد كيف يعمل
              </Button>
            </a>
          </div>

          <div className="mt-10 flex items-center justify-center lg:justify-start gap-8 text-sm text-muted-foreground">
            <div>
              <div className="text-2xl font-bold text-navy">+12K</div>
              <div>مستقل يستخدم مالي</div>
            </div>
            <div className="w-px h-10 bg-border" />
            <div>
              <div className="text-2xl font-bold text-navy flex items-center gap-1">
                ٤.٩
                <Star className="w-5 h-5 fill-warning text-warning" />
              </div>
              <div>تقييم المستخدمين</div>
            </div>
          </div>
        </div>

        {/* Illustration */}
        <div className="relative animate-fade-up" style={{ animationDelay: "150ms" }}>
          <div className="absolute inset-5 border border-border bg-card/60 shadow-card" aria-hidden="true" />
          <img
            src={heroImage}
            alt="مستقل يدير أعماله المالية على لوحة تحكم مالي"
            width={1024}
            height={1024}
            className="relative w-full max-w-lg mx-auto p-4"
          />

          {/* Floating mini cards */}
          <div className="hidden md:flex absolute top-8 -left-4 bg-card rounded-lg border border-border shadow-card p-4 items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-teal-soft text-teal grid place-items-center">
              <Sparkles className="w-5 h-5" />
            </div>
            <div>
              <div className="text-xs text-muted-foreground">اقتراح ذكي</div>
              <div className="text-sm font-bold text-navy">ارفع سعرك +18٪</div>
            </div>
          </div>

          <div className="hidden md:flex absolute bottom-20 -left-4 bg-card rounded-lg border border-border shadow-card p-4 items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-success-soft text-success grid place-items-center">
              <ShieldCheck className="w-5 h-5" />
            </div>
            <div>
              <div className="text-xs text-muted-foreground">تحليل العميل</div>
              <div className="text-sm font-bold text-navy">خطورة منخفضة</div>
            </div>
          </div>

          <div className="hidden md:flex absolute bottom-4 -right-2 bg-card rounded-lg border border-border shadow-card p-4 items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-teal-soft text-teal grid place-items-center">
              <Brain className="w-5 h-5" />
            </div>
            <div>
              <div className="text-xs text-muted-foreground">صافي الربح</div>
              <div className="text-sm font-bold text-navy">٦٧٪ هامش</div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
