import { Link } from "react-router-dom";
import { Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import { PRICING } from "@/data/mali";

export default function Pricing() {
  return (
    <section id="pricing" className="py-24 bg-muted/45 border-y border-border">
      <div className="container">
        <div className="text-center max-w-2xl mx-auto mb-16">
          <span className="text-teal text-sm font-semibold tracking-wide">الأسعار</span>
          <h2 className="text-3xl md:text-4xl font-bold text-navy mt-2">
            خطط بسيطة، بدون مفاجآت
          </h2>
          <p className="mt-4 text-muted-foreground">
            ابدأ مجاناً وارتقِ عندما تنمو. ألغِ في أي وقت.
          </p>
        </div>

        <div className="grid md:grid-cols-3 gap-6 max-w-5xl mx-auto">
          {PRICING.map((p) => (
            <div
              key={p.name}
              className={`rounded-lg p-8 border transition-shadow ${
                p.highlighted
                  ? "bg-navy text-white border-navy shadow-elevated relative"
                  : "bg-card border-border shadow-card"
              }`}
            >
              {p.highlighted && (
                <span className="absolute -top-3 right-1/2 translate-x-1/2 bg-card text-navy border border-border text-xs font-bold px-3 py-1 rounded-lg shadow-card">
                  الأكثر شعبية
                </span>
              )}
              <h3 className={`font-bold text-lg ${p.highlighted ? "text-white" : "text-navy"}`}>
                {p.name}
              </h3>
              <div className="mt-4 flex items-baseline gap-2">
                <span className={`text-4xl font-bold ${p.highlighted ? "text-white" : "text-navy"}`}>
                  {p.price}
                </span>
                <span className={`text-sm ${p.highlighted ? "text-white/70" : "text-muted-foreground"}`}>
                  {p.period}
                </span>
              </div>

              <ul className="mt-6 space-y-3">
                {p.features.map((f) => (
                  <li key={f} className="flex items-start gap-2 text-sm">
                    <Check
                      className={`w-4 h-4 mt-0.5 flex-shrink-0 ${
                        p.highlighted ? "text-teal-light" : "text-teal"
                      }`}
                    />
                    <span className={p.highlighted ? "text-white/90" : "text-navy"}>{f}</span>
                  </li>
                ))}
              </ul>

              {p.cta === "تواصل معنا" ? (
                <a href="#contact" className="block mt-8">
                  <Button
                    className={`w-full rounded-lg ${
                      p.highlighted
                        ? "bg-white text-navy hover:bg-muted shadow-glow"
                        : "bg-navy hover:bg-navy-light text-white"
                    }`}
                  >
                    {p.cta}
                  </Button>
                </a>
              ) : (
                <Link to="/register" className="block mt-8">
                  <Button
                    className={`w-full rounded-lg ${
                      p.highlighted
                        ? "bg-white text-navy hover:bg-muted shadow-glow"
                        : "bg-navy hover:bg-navy-light text-white"
                    }`}
                  >
                    {p.cta}
                  </Button>
                </Link>
              )}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
