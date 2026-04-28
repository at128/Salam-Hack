import { TESTIMONIALS } from "@/data/mali";

export default function Testimonials() {
  return (
    <section id="testimonials" className="py-24 bg-background">
      <div className="container">
        <div className="text-center max-w-2xl mx-auto mb-16">
          <span className="text-teal text-sm font-semibold tracking-wide">آراء العملاء</span>
          <h2 className="text-3xl md:text-4xl font-bold text-navy mt-2">
            مستقلون يثقون بمالي يومياً
          </h2>
        </div>

        <div className="grid md:grid-cols-3 gap-6">
          {TESTIMONIALS.map((t) => (
            <figure
              key={t.name}
              className="bg-card rounded-lg p-7 border border-border shadow-card relative"
            >
              <div className="text-teal text-4xl leading-none mb-3">"</div>
              <blockquote className="text-navy leading-relaxed">{t.quote}</blockquote>
              <figcaption className="mt-6 flex items-center gap-3">
                <div className="w-11 h-11 rounded-lg bg-navy text-white grid place-items-center font-bold">
                  {t.name.charAt(0)}
                </div>
                <div>
                  <div className="font-semibold text-navy text-sm">{t.name}</div>
                  <div className="text-xs text-muted-foreground">{t.role}</div>
                </div>
              </figcaption>
            </figure>
          ))}
        </div>
      </div>
    </section>
  );
}
