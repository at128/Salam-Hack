import { ArrowDownRight, ArrowUpRight, Minus, type LucideIcon } from "lucide-react";

type Props = {
  label: string;
  value: string;
  delta: string;
  positive: boolean;
  trend?: "up" | "down" | "neutral";
  icon: LucideIcon;
};

export default function KpiCard({ label, value, delta, positive, trend, icon: Icon }: Props) {
  const resolvedTrend = trend ?? (positive ? "up" : "down");

  return (
    <div className="bg-card rounded-2xl p-5 border border-border/70 shadow-card">
      <div className="flex items-start justify-between">
        <div className="w-11 h-11 rounded-xl bg-teal-soft text-teal grid place-items-center">
          <Icon className="w-5 h-5" />
        </div>
        <span
          className={`inline-flex items-center gap-1 text-xs font-bold px-2 py-1 rounded-full ${
            resolvedTrend === "up"
              ? "bg-success-soft text-success"
              : resolvedTrend === "neutral"
              ? "bg-muted text-muted-foreground"
              : "bg-warning-soft text-warning"
          }`}
        >
          {resolvedTrend === "up" ? (
            <ArrowUpRight className="w-3 h-3" />
          ) : resolvedTrend === "neutral" ? (
            <Minus className="w-3 h-3" />
          ) : (
            <ArrowDownRight className="w-3 h-3" />
          )}
          {delta}
        </span>
      </div>
      <div className="mt-4 text-2xl font-bold text-navy">{value}</div>
      <div className="text-xs text-muted-foreground mt-1">{label}</div>
    </div>
  );
}
