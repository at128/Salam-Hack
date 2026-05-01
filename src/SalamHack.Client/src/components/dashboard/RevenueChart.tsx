export default function RevenueChart({
  data
}: {
  data: { monthName: string; revenue: number; expenses: number }[];
}) {
  const safeData = data.map((item) => ({
    ...item,
    revenue: Number.isFinite(Number(item.revenue)) ? Number(item.revenue) : 0,
    expenses: Number.isFinite(Number(item.expenses)) ? Number(item.expenses) : 0,
  }));
  const max = Math.max(...safeData.map(d => Math.max(d.revenue, d.expenses)), 100);
  const chartH = 180;
  const barW = 26;
  const gap = 48;
  const totalW = Math.max(safeData.length * (barW * 2 + gap), 10);

  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <div className="flex items-center justify-between mb-5">
        <div>
          <h3 className="font-bold text-navy">الإيراد مقابل المصاريف</h3>
          <p className="text-xs text-muted-foreground">آخر فترة</p>
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

      {safeData.length === 0 ? (
        <div className="flex h-[220px] items-center justify-center rounded-xl bg-muted/20 text-xs text-muted-foreground">
          لا توجد بيانات كافية للرسم البياني
        </div>
      ) : (
        <svg
          width="100%"
          viewBox={`0 0 ${totalW + 20} ${chartH + 40}`}
          style={{ direction: "ltr" }}
        >
          {safeData.map((m, i) => {
            const x = i * (barW * 2 + gap) + 10;
            const rH = (m.revenue / max) * chartH;
            const eH = (m.expenses / max) * chartH;
            return (
              <g key={m.monthName + i}>
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
                  {m.monthName}
                </text>
              </g>
            );
          })}
        </svg>
      )}
    </div>
  );
}
