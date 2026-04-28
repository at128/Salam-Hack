import { Sparkles, ShieldAlert, Brain } from "lucide-react";

const FEATURES = [
  {
    icon: Sparkles,
    iconBg: "bg-card border border-border",
    iconColor: "text-teal",
    accentBar: "bg-navy",
    tag: "التسعير الذكي",
    tagBg: "bg-card text-navy border border-border",
    headline: "بكم تسعّر خدمتك؟",
    desc: "اقتراحات أسعار مبنية على ساعاتك وهامش ربحك الفعلي — لا تخمين.",
    stat: "+18٪",
    statLabel: "متوسط الزيادة الموصى بها",
    bullets: ["سعر مقترح لكل خدمة", "هامش ربح محسوب تلقائياً", "تنبيه حين تكون أسعارك منخفضة"],
  },
  {
    icon: ShieldAlert,
    iconBg: "bg-card border border-border",
    iconColor: "text-teal",
    accentBar: "bg-teal",
    tag: "تحليل العميل",
    tagBg: "bg-card text-navy border border-border",
    headline: "هل يستحق وقتك؟",
    desc: "أجب على ٥ أسئلة واحصل فوراً على مستوى خطورة التعامل مع العميل.",
    stat: "٥ أسئلة",
    statLabel: "تكشف مستوى الخطورة",
    bullets: ["تحليل فوري للمخاطر", "مبني على الدفع والتواصل والميزانية", "توصيات: تعامل، احتط، أو تجنّب"],
  },
  {
    icon: Brain,
    iconBg: "bg-card border border-border",
    iconColor: "text-teal",
    accentBar: "bg-navy",
    tag: "محلل الأرباح الذكي",
    tagBg: "bg-card text-navy border border-border",
    headline: "هل أنت فعلاً رابح؟",
    desc: "اكتشف أي خدمة تستنزفك وأين يذهب مالك بدقة — لا بالتخمين.",
    stat: "٦٧٪",
    statLabel: "متوسط هامش الربح المكتشف",
    bullets: ["رؤى مبنية على بياناتك", "كشف الخدمات غير المربحة", "أفضل توقيت لتحصيل فواتيرك"],
  },
];

export default function Features() {
  return (
    <section id="features" className="py-24 bg-background">
      <div className="container">
        <div className="text-center max-w-xl mx-auto mb-16">
          <span className="text-teal text-sm font-semibold tracking-wide">المشاكل التي نحلّها</span>
          <h2 className="text-3xl md:text-4xl font-bold text-navy mt-2">
            ٣ أسئلة يسألها كل مستقل
          </h2>
          <p className="mt-4 text-muted-foreground">
            مالي مصمّم حول مشاكلك الحقيقية — لا مزايا عشوائية، بل حلول مركّزة.
          </p>
        </div>

        <div className="grid md:grid-cols-3 gap-6">
          {FEATURES.map((f) => {
            const Icon = f.icon;
            return (
              <article
                key={f.tag}
                className="group bg-card rounded-lg border border-border shadow-card hover:shadow-elevated transition-shadow overflow-hidden"
              >
                <div className={`h-1 w-full ${f.accentBar}`} />

                <div className="p-6">
                  {/* Icon + tag */}
                  <div className="flex items-center justify-between mb-5">
                    <span className={`text-xs font-semibold px-3 py-1 rounded-lg ${f.tagBg}`}>
                      {f.tag}
                    </span>
                    <div className={`w-10 h-10 rounded-lg grid place-items-center ${f.iconBg} ${f.iconColor}`}>
                      <Icon className="w-5 h-5" />
                    </div>
                  </div>

                  {/* Headline + desc */}
                  <h3 className="text-xl font-bold text-navy">{f.headline}</h3>
                  <p className="mt-2 text-sm text-muted-foreground leading-relaxed">{f.desc}</p>

                  {/* Stat */}
                  <div className="mt-5 py-4 border-y border-border/50 flex items-baseline gap-2">
                    <span className="text-3xl font-bold text-navy">{f.stat}</span>
                    <span className="text-xs text-muted-foreground">{f.statLabel}</span>
                  </div>

                  {/* Bullets */}
                  <ul className="mt-4 space-y-2">
                    {f.bullets.map((b) => (
                      <li key={b} className="flex items-center gap-2 text-sm text-navy">
                        <span className="w-1.5 h-1.5 bg-teal shrink-0" />
                        {b}
                      </li>
                    ))}
                  </ul>
                </div>
              </article>
            );
          })}
        </div>
      </div>
    </section>
  );
}
