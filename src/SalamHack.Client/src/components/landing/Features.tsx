import { Brain, ChartNoAxesCombined, FileText, HandCoins, ShieldCheck, WalletCards } from "lucide-react";

const FEATURES = [
  {
    icon: Brain,
    tag: "تحليل الربح الحقيقي",
    headline: "اعرف الربح خلف كل مشروع",
    desc: "مالي يحلل المصاريف، ساعات العمل، المدفوعات، والتكاليف المخفية ليكشف صافي الربح الحقيقي لكل مشروع.",
    stat: "صافي",
    statLabel: "ربح واضح لكل مشروع",
    bullets: ["ربحية مفصلة حسب المشروع", "كشف الخدمات التي تستنزف هامشك", "مقارنة الإيراد بالتكلفة الفعلية"],
  },
  {
    icon: ChartNoAxesCombined,
    tag: "تسعير أذكى",
    headline: "سعّر بثقة لا بتخمين",
    desc: "افهم هل أسعارك مناسبة فعلًا، ومتى تحتاج ترفع السعر أو تعيد حساب هامش الربح.",
    stat: "+وعي",
    statLabel: "في قرارات التسعير",
    bullets: ["تحليل أثر السعر على الهامش", "تنبيه عند انخفاض الربحية", "توصيات لتحسين التسعير"],
  },
  {
    icon: WalletCards,
    tag: "تدفق نقدي واضح",
    headline: "تابع أموالك قبل أن تتعطل",
    desc: "راقب الفواتير، المصاريف، المدفوعات المتأخرة، والتزاماتك القادمة من مكان واحد.",
    stat: "تدفق",
    statLabel: "نقدي مفهوم",
    bullets: ["متابعة المدفوعات المتأخرة", "رؤية للمصاريف القادمة", "تنظيم الفواتير والتحصيل"],
  },
  {
    icon: HandCoins,
    tag: "ربحية العملاء",
    headline: "اعرف العميل الأكثر قيمة",
    desc: "مالي يساعدك على معرفة العملاء الأكثر ربحًا والعملاء الذين يستهلكون وقتك بدون عائد مناسب.",
    stat: "أذكى",
    statLabel: "في اختيار العملاء",
    bullets: ["ترتيب العملاء حسب الربحية", "رصد العملاء كثيري التأخير", "قرار أوضح للتجديد أو الرفض"],
  },
  {
    icon: FileText,
    tag: "فواتير ومصاريف",
    headline: "كل مستند مالي في مكانه",
    desc: "نظّم فواتيرك ومصاريفك واربطها بالمشاريع والعملاء لتتحول البيانات إلى صورة مالية مفهومة.",
    stat: "منظم",
    statLabel: "بدون تعقيد",
    bullets: ["تصنيف المصاريف حسب المشروع", "متابعة حالة الفاتورة", "سجل مالي واضح"],
  },
  {
    icon: ShieldCheck,
    tag: "قرارات موثوقة",
    headline: "حوّل الأرقام إلى قرارات",
    desc: "بدل التقارير الجامدة، يقدم مالي توصيات تساعدك على تقليل المصاريف، تحسين الهامش، والنمو بوعي.",
    stat: "قرار",
    statLabel: "أوضح من كل رقم",
    bullets: ["تنبيهات عند ضعف الهامش", "اقتراحات لتحسين الربحية", "رؤية عملية لنمو مستدام"],
  },
];

export default function Features() {
  return (
    <section id="features" className="py-24 bg-background">
      <div className="container">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <span className="text-teal text-sm font-semibold tracking-wide">المميزات</span>
          <h2 className="text-3xl md:text-4xl font-bold text-navy mt-2">
            كل ما تحتاجه لإدارة مالية ذكية
          </h2>
          <p className="mt-4 text-muted-foreground leading-8">
            مالي يعيد تعريف إدارة أموال المستقلين وأصحاب الأعمال الصغيرة؛ ينظم الفواتير والمصاريف،
            ويكشف الربح الحقيقي، ويحلل التسعير والمدفوعات والتدفق النقدي وربحية العملاء بناءً على بياناتك الفعلية.
          </p>
        </div>

        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {FEATURES.map((feature, index) => {
            const Icon = feature.icon;
            return (
              <article
                key={feature.tag}
                className="group bg-card rounded-lg border border-border shadow-card hover:shadow-elevated transition-shadow overflow-hidden"
              >
                <div className={`h-1 w-full ${index % 2 === 0 ? "bg-navy" : "bg-teal"}`} />

                <div className="p-6">
                  <div className="flex items-center justify-between mb-5">
                    <span className="text-xs font-semibold px-3 py-1 rounded-lg bg-card text-navy border border-border">
                      {feature.tag}
                    </span>
                    <div className="w-10 h-10 rounded-lg grid place-items-center bg-card border border-border text-teal">
                      <Icon className="w-5 h-5" />
                    </div>
                  </div>

                  <h3 className="text-xl font-bold text-navy">{feature.headline}</h3>
                  <p className="mt-2 text-sm text-muted-foreground leading-relaxed">{feature.desc}</p>

                  <div className="mt-5 py-4 border-y border-border/50 flex items-baseline gap-2">
                    <span className="text-3xl font-bold text-navy">{feature.stat}</span>
                    <span className="text-xs text-muted-foreground">{feature.statLabel}</span>
                  </div>

                  <ul className="mt-4 space-y-2">
                    {feature.bullets.map((bullet) => (
                      <li key={bullet} className="flex items-center gap-2 text-sm text-navy">
                        <span className="w-1.5 h-1.5 bg-teal shrink-0" />
                        {bullet}
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