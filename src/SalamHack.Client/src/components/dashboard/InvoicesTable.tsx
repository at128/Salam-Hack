import { INVOICES } from "@/data/mali";

const statusColor: Record<string, string> = {
  مدفوعة: "bg-success-soft text-success",
  مرسلة: "bg-teal-soft text-teal",
  متأخرة: "bg-warning-soft text-warning",
  مسودة: "bg-muted text-muted-foreground",
};

export default function InvoicesTable() {
  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <div className="flex items-center justify-between mb-4">
        <h3 className="font-bold text-navy">آخر الفواتير</h3>
        <button className="text-xs text-teal font-semibold hover:underline">عرض الكل</button>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="text-xs text-muted-foreground">
            <tr className="border-b border-border/60">
              <th className="text-right font-medium py-3">#</th>
              <th className="text-right font-medium py-3">العميل</th>
              <th className="text-right font-medium py-3">الخدمة</th>
              <th className="text-right font-medium py-3">المبلغ</th>
              <th className="text-right font-medium py-3">الحالة</th>
            </tr>
          </thead>
          <tbody>
            {INVOICES.map((inv) => (
              <tr key={inv.id} className="border-b border-border/40 last:border-0">
                <td className="py-3 font-bold text-teal">#{inv.id}</td>
                <td className="py-3 text-navy">{inv.client}</td>
                <td className="py-3 text-muted-foreground">{inv.service}</td>
                <td className="py-3 font-semibold text-navy">{inv.total.toLocaleString()} ر.س</td>
                <td className="py-3">
                  <span
                    className={`inline-block px-2.5 py-1 rounded-full text-[11px] font-semibold ${
                      statusColor[inv.status] || "bg-muted text-muted-foreground"
                    }`}
                  >
                    {inv.status}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}