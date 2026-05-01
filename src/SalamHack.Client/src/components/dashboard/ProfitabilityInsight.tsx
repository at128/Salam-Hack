import { useEffect, useState } from "react";
import { Lightbulb, Loader2 } from "lucide-react";
import { fetchProfitabilityReport } from "@/lib/reports";

interface Props {
  fromUtc?: string;
  toUtc?: string;
}

export default function ProfitabilityInsight({ fromUtc, toUtc }: Props) {
  const [insight, setInsight] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    
    fetchProfitabilityReport(fromUtc, toUtc)
      .then((report) => {
        setInsight(report.insight || "");
      })
      .catch(() => setInsight(""))
      .finally(() => setLoading(false));
  }, [fromUtc, toUtc]);

  if (loading) {
    return (
      <div className="bg-blue-50 rounded-xl p-4 border border-blue-200 flex items-center gap-3">
        <Loader2 className="w-5 h-5 animate-spin text-blue-600" />
        <span className="text-sm text-blue-900">جاري تحميل التحليلات...</span>
      </div>
    );
  }

  if (!insight) {
    return null;
  }

  return (
    <div className="bg-blue-50 rounded-xl p-4 border border-blue-200 flex items-start gap-3">
      <Lightbulb className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
      <div>
        <p className="text-sm text-blue-900 font-medium">نصيحة تحليلية</p>
        <p className="text-sm text-blue-800 mt-1">{insight}</p>
      </div>
    </div>
  );
}
