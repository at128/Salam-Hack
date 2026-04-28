import { MONTHS, REVENUE_DATA, EXPENSE_DATA } from "@/data/mali";

export default function RevenueChart() {
  const max = Math.max(...REVENUE_DATA, ...EXPENSE_DATA);
  const chartH = 180;
  const barW = 26;
  const gap = 48;
  const totalW = MONTHS.length * (barW * 2 + gap);

  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <div className="flex items-center justify-between mb-5">
        <div>
          <h3 className="font-bold text-navy">الإيراد مقابل المصاريف</h3>
          <p className="text-xs text-muted-foreground">آخر ٦ أشهر</p>
        </div>
        <div className="flex gap-4 text-xs">
          <span className="flex items-center gap-1.5">
            <span className="w-3 h-3 rounded-sm bg-teal" /> الإيراد
          </span>
          <span className="flex items-center gap-1.5">
            <span className="w-3 h-3 rounded-sm bg-navy/30" /> المصاريف
          </span>
        </div>
      </div>

      <svg
        width="100%"
        viewBox={`0 0 ${totalW + 20} ${chartH + 40}`}
        style={{ direction: "ltr" }}
      >
        {MONTHS.map((m, i) => {
          const x = i * (barW * 2 + gap) + 10;
          const rH = (REVENUE_DATA[i] / max) * chartH;
          const eH = (EXPENSE_DATA[i] / max) * chartH;
          return (
            <g key={m}>
              <rect
                x={x}
                y={chartH - rH}
                width={barW}
                height={rH}
                rx={6}
                className="fill-teal"
              />
              <rect
                x={x + barW + 4}
                y={chartH - eH}
                width={barW}
                height={eH}
                rx={6}
                className="fill-navy/25"
              />
              <text
                x={x + barW}
                y={chartH + 22}
                textAnchor="middle"
                fontSize="11"
                className="fill-muted-foreground"
              >
                {m}
              </text>
            </g>
          );
        })}
      </svg>
    </div>
  );
}