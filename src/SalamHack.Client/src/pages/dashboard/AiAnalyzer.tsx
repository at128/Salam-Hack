import { PageHeader } from "@/components/dashboard/DashboardLayout";
import AlertsPanel from "@/components/dashboard/AlertsPanel";
import { Bot } from "lucide-react";

const insights = [
  "خدمة «تصميم الشعارات» هامشها ٢٥٪ فقط — فكّر برفع السعر أو تقليل ساعات التنفيذ.",
  "٧٢٪ من إيرادك يأتي من ٣ عملاء فقط. وسّع قاعدة عملائك لتقليل المخاطر.",
  "أفضل أيام تحصيلك هي الأحد والإثنين — أرسل فواتيرك الخميس لتحصيل أسرع.",
];

export default function AiAnalyzerPage() {
  return (
    <>
      <PageHeader title="محلل الأرباح الذكي" desc="رؤى مبنية على بياناتك الفعلية لتنمية دخلك." />
      <div className="grid lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-card rounded-2xl p-6 border border-border/70 shadow-card">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-11 h-11 rounded-xl bg-gradient-brand text-white grid place-items-center shadow-glow">
              <Bot className="w-5 h-5" />
            </div>
            <div>
              <h3 className="font-bold text-navy">توصيات مالي</h3>
              <p className="text-xs text-muted-foreground">محدّثة بناءً على آخر ٣٠ يوم</p>
            </div>
          </div>
          <ul className="space-y-3">
            {insights.map((t, i) => (
              <li key={i} className="flex gap-3 p-3 rounded-xl bg-muted/40 text-sm text-navy leading-relaxed">
                <span className="w-6 h-6 rounded-full bg-teal text-white grid place-items-center text-xs font-bold shrink-0">
                  {i + 1}
                </span>
                {t}
              </li>
            ))}
          </ul>
        </div>
        <AlertsPanel />
      </div>
    </>
  );
}