import { Mail, MessageSquare, Phone } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

export default function ContactSection() {
  return (
    <section id="contact" className="py-24 bg-background">
      <div className="container">
        <div className="text-center max-w-2xl mx-auto mb-16">
          <span className="text-teal text-sm font-semibold tracking-wide">تواصل معنا</span>
          <h2 className="text-3xl md:text-4xl font-bold text-navy mt-2">
            نحن هنا للمساعدة
          </h2>
          <p className="mt-4 text-muted-foreground">
            هل لديك سؤال أو تحتاج مساعدة؟ تواصل معنا وسنرد عليك في أقرب وقت.
          </p>
        </div>

        <div className="grid md:grid-cols-2 gap-12 max-w-4xl mx-auto">
          <div className="space-y-8">
            <div className="flex items-start gap-4">
              <div className="w-11 h-11 rounded-lg bg-card border border-border grid place-items-center flex-shrink-0">
                <Mail className="w-5 h-5 text-teal" />
              </div>
              <div>
                <h3 className="font-semibold text-navy">البريد الإلكتروني</h3>
                <p className="text-muted-foreground text-sm mt-1">hello@mali.app</p>
              </div>
            </div>

            <div className="flex items-start gap-4">
              <div className="w-11 h-11 rounded-lg bg-card border border-border grid place-items-center flex-shrink-0">
                <MessageSquare className="w-5 h-5 text-teal" />
              </div>
              <div>
                <h3 className="font-semibold text-navy">الدردشة المباشرة</h3>
                <p className="text-muted-foreground text-sm mt-1">متاحون من الأحد إلى الخميس، ٩ص – ٦م</p>
              </div>
            </div>

            <div className="flex items-start gap-4">
              <div className="w-11 h-11 rounded-lg bg-card border border-border grid place-items-center flex-shrink-0">
                <Phone className="w-5 h-5 text-teal" />
              </div>
              <div>
                <h3 className="font-semibold text-navy">الهاتف</h3>
                <p className="text-muted-foreground text-sm mt-1">‎+966 50 000 0000</p>
              </div>
            </div>
          </div>

          <form className="space-y-4" onSubmit={(e) => e.preventDefault()}>
            <div className="grid grid-cols-2 gap-4">
              <Input placeholder="الاسم" className="rounded-lg bg-card border-border" />
              <Input type="email" placeholder="البريد الإلكتروني" className="rounded-lg bg-card border-border" />
            </div>
            <Input placeholder="الموضوع" className="rounded-lg bg-card border-border" />
            <Textarea
              placeholder="رسالتك..."
              className="rounded-lg min-h-[120px] resize-none bg-card border-border"
            />
            <Button
              type="submit"
              className="w-full rounded-lg bg-navy hover:bg-navy-light shadow-glow"
            >
              إرسال الرسالة
            </Button>
          </form>
        </div>
      </div>
    </section>
  );
}
