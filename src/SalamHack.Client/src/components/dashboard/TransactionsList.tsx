import { TRANSACTIONS } from "@/data/mali";
import { ArrowDown, ArrowUp } from "lucide-react";

export default function TransactionsList() {
  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <div className="flex items-center justify-between mb-4">
        <h3 className="font-bold text-navy">آخر المعاملات</h3>
        <button className="text-xs text-teal font-semibold hover:underline">
          عرض الكل
        </button>
      </div>
      <div className="divide-y divide-border/60">
        {TRANSACTIONS.map((tx) => {
          const income = tx.type === "income";
          return (
            <div key={tx.id} className="flex items-center justify-between py-3">
              <div className="flex items-center gap-3">
                <div
                  className={`w-9 h-9 rounded-xl grid place-items-center ${
                    income ? "bg-success-soft text-success" : "bg-danger-soft text-danger"
                  }`}
                >
                  {income ? <ArrowDown className="w-4 h-4" /> : <ArrowUp className="w-4 h-4" />}
                </div>
                <div>
                  <div className="text-sm font-medium text-navy">{tx.name}</div>
                  <div className="text-xs text-muted-foreground">{tx.date}</div>
                </div>
              </div>
              <div className="flex items-center gap-2">
                {tx.status === "متأخرة" && (
                  <span className="text-[11px] bg-warning-soft text-warning px-2 py-0.5 rounded-full font-semibold">
                    متأخرة
                  </span>
                )}
                <span
                  className={`font-bold text-sm ${
                    income ? "text-success" : "text-danger"
                  }`}
                >
                  {income ? "+" : ""}
                  {tx.amount.toLocaleString()} ر.س
                </span>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}