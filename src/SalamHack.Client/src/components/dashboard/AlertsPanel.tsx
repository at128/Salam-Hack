import { ALERTS } from "@/data/mali";
import { Button } from "@/components/ui/button";

const styles = {
  warning: "bg-warning-soft text-warning border-warning",
  info: "bg-teal-soft text-teal border-teal",
  success: "bg-success-soft text-success border-success",
} as const;

export default function AlertsPanel() {
  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <h3 className="font-bold text-navy mb-4">تنبيهات ذكية</h3>
      <div className="space-y-2">
        {ALERTS.map((a, i) => {
          const Icon = a.icon;
          return (
          <div
            key={i}
            className={`flex items-center gap-3 p-3 rounded-xl border-r-4 ${styles[a.type]}`}
          >
            <Icon className="w-5 h-5 shrink-0" />
            <span className="flex-1 text-sm text-navy leading-snug">{a.text}</span>
            {a.action && (
              <Button size="sm" variant="outline" className="text-xs h-7 rounded-full bg-white">
                {a.action}
              </Button>
            )}
          </div>
          );
        })}
      </div>
    </div>
  );
}