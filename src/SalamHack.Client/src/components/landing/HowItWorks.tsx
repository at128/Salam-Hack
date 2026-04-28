import { Sparkles, ShieldAlert, Brain } from "lucide-react";

const STEPS = [
  {
    n: "١",
    icon: Sparkles,
    iconBg: "bg-card",
    iconColor: "text-teal",
    title: "سعّر خدماتك بذكاء",
    desc: "أدخل ساعاتك وتكاليفك — مالي يقترح لك السعر المثالي لكل خدمة بناءً على هامش الربح الفعلي.",
    tag: "التسعير الذكي",
    tagBg: "bg-card text-navy border border-border",
  },
  {
    n: "٢",
    icon: ShieldAlert,
    iconBg: "bg-card",
    iconColor: "text-teal",
    title: "حلّل العميل قبل أن تبدأ",
    desc: "أجب على ٥ أسئلة عن العميل واحصل فوراً على مستوى الخطورة وتوصيات واضحة للتعامل معه.",
    tag: "تحليل العميل",
    tagBg: "bg-card text-navy border border-border",
  },
  {
    n: "٣",
    icon: Brain,
    iconBg: "bg-card",
    iconColor: "text-teal",
    title: "اكتشف ربحك الحقيقي",
    desc: "محلل الأرباح الذكي يكشف لك أي خدمة تستنزفك وأين يذهب مالك — لتتخذ قرارات مبنية على بيانات.",
    tag: "محلل الأرباح الذكي",
    tagBg: "bg-card text-navy border border-border",
  },
];

export default function HowItWorks() {
  return (
    <section id="how" className="py-24 bg-muted/45 border-y border-border">
      <div className="container">
        <div className="text-center max-w-2xl mx-auto mb-16">
          <span className="text-teal text-sm font-semibold tracking-wide">كيف يعمل</span>
          <h2 className="text-3xl md:text-4xl font-bold text-navy mt-2">
            ٣ أدوات، ٣ قرارات تغيّر مسارك
          </h2>
          <p className="mt-4 text-muted-foreground leading-relaxed">
            كل أداة تحل مشكلة محددة — وكلها تعمل معاً لتعطيك صورة كاملة عن نشاطك.
          </p>
        </div>

        <div className="grid md:grid-cols-3 gap-6 relative">
          <div
            className="hidden md:block absolute border-t-2 border-dashed border-navy/20"
            style={{
              top: "3.75rem",
              left: "calc((100% - 3rem) / 6)",
              right: "calc((100% - 3rem) / 6)",
            }}
            aria-hidden="true"
          />
          {STEPS.map((s) => {
            const Icon = s.icon;
            return (
              <div key={s.n} className="relative bg-card rounded-lg p-8 border border-border shadow-card text-center">
                <div className="w-14 h-14 mx-auto rounded-lg bg-navy text-white grid place-items-center text-xl font-bold shadow-glow">
                  {s.n}
                </div>
                <span className={`inline-block mt-4 text-xs font-semibold px-3 py-1 rounded-lg ${s.tagBg}`}>
                  {s.tag}
                </span>
                <h3 className="mt-3 font-bold text-navy text-lg">{s.title}</h3>
                <p className="mt-2 text-sm text-muted-foreground leading-relaxed">{s.desc}</p>
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}
