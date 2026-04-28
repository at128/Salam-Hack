import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";

export default function CTASection() {
  return (
    <section className="py-20">
      <div className="container">
        <div className="relative overflow-hidden rounded-lg bg-card border border-border p-12 md:p-16 text-center shadow-card">
          <div className="relative z-10">
            <h2 className="text-3xl md:text-4xl font-bold text-navy">
              جاهز تتحكم بماليتك؟
            </h2>
            <p className="mt-4 text-muted-foreground max-w-xl mx-auto">
              انضم لآلاف المستقلين العرب الذين يديرون أعمالهم بثقة وذكاء مع مالي.
            </p>
            <Link to="/register" className="inline-block mt-8">
              <Button
                size="lg"
                className="rounded-lg bg-navy text-white hover:bg-navy-light px-8 h-12 text-base font-bold shadow-glow"
              >
                ابدأ تجربتك المجانية
                <ArrowLeft className="w-4 h-4 mr-1" />
              </Button>
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}
