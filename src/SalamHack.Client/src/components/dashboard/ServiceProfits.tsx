import { SERVICE_PROFITS } from "@/data/mali";

export default function ServiceProfits() {
  const max = Math.max(...SERVICE_PROFITS.map((s) => s.profit));
  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <h3 className="font-bold text-navy mb-1">الأرباح حسب الخدمة</h3>
      <p className="text-xs text-muted-foreground mb-5">
        أي خدماتك تجلب أعلى ربحية؟
      </p>
      <div className="space-y-4">
        {SERVICE_PROFITS.map((s) => (
          <div key={s.name}>
            <div className="flex items-center justify-between text-sm mb-1.5">
              <span className="text-navy font-medium">{s.name}</span>
              <div className="flex items-center gap-2">
                <span className="text-xs text-muted-foreground">{s.margin}٪</span>
                <span className="font-bold text-navy">{s.profit.toLocaleString()} ر.س</span>
              </div>
            </div>
            <div className="h-2 rounded-full bg-muted overflow-hidden">
              <div
                className="h-full bg-gradient-brand rounded-full"
                style={{ width: `${(s.profit / max) * 100}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}