import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { Sparkles, TrendingUp, Clock } from "lucide-react";
import { SERVICE_PROFITS } from "@/data/mali";

export default function PricingPage() {
  return (
    <>
      <PageHeader title="التسعير الذكي" desc="اقتراحات أسعار مبنية على ساعاتك الفعلية وهامش ربحك." />
      <div className="grid md:grid-cols-3 gap-4 mb-6">
        {[
          { icon: Sparkles, label: "اقتراح ذكي", value: "+18٪", hint: "زيادة موصى بها على أسعارك الحالية" },
          { icon: TrendingUp, label: "متوسط هامش الربح", value: "٦٧٪", hint: "عبر آخر ٣ أشهر" },
          { icon: Clock, label: "متوسط ساعة العمل", value: "185 ر.س", hint: "محسوب من مشاريعك" },
        ].map((c) => (
          <div key={c.label} className="bg-card rounded-2xl p-5 border border-border/70 shadow-card">
            <div className="w-11 h-11 rounded-xl bg-teal-soft text-teal grid place-items-center">
              <c.icon className="w-5 h-5" />
            </div>
            <div className="mt-4 text-2xl font-bold text-navy">{c.value}</div>
            <div className="text-xs text-muted-foreground mt-1">{c.label}</div>
            <div className="text-xs text-muted-foreground/80 mt-2">{c.hint}</div>
          </div>
        ))}
      </div>

      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <h3 className="font-bold text-navy mb-4">سعر مقترح لكل خدمة</h3>
        <div className="space-y-3">
          {SERVICE_PROFITS.map((s) => (
            <div key={s.name} className="flex items-center justify-between py-2 border-b border-border/40 last:border-0">
              <div className="text-sm text-navy font-medium">{s.name}</div>
              <div className="flex items-center gap-3">
                <span className="text-xs text-muted-foreground">هامش {s.margin}٪</span>
                <span className="text-sm font-bold text-teal">{(s.profit / 10).toFixed(0)} ر.س / ساعة</span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </>
  );
}