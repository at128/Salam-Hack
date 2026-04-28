import { Link, NavLink } from "react-router-dom";
import {
  ArrowLeftRight,
  Brain,
  FileText,
  LayoutDashboard,
  LogOut,
  PieChart,
  ShieldAlert,
  Sparkles,
  TrendingUp,
  UserRound,
  Wallet,
} from "lucide-react";
import type { AuthUser } from "@/lib/auth";

const items = [
  { id: "dashboard", label: "لوحة التحكم", icon: LayoutDashboard, href: "/dashboard" },
  { id: "pricing", label: "التسعير الذكي", icon: Sparkles, href: "/dashboard/pricing" },
  { id: "client-risk", label: "تحليل العميل", icon: ShieldAlert, href: "/dashboard/client-risk" },
  { id: "invoices", label: "الفواتير", icon: FileText, href: "/dashboard/invoices" },
  { id: "payments", label: "المدفوعات", icon: Wallet, href: "/dashboard/payments" },
  { id: "expenses", label: "كشف الربح الحقيقي", icon: TrendingUp, href: "/dashboard/profit" },
  { id: "profits", label: "أين ذهب ربحك؟", icon: PieChart, href: "/dashboard/breakdown" },
  { id: "cashflow", label: "التدفق النقدي", icon: ArrowLeftRight, href: "/dashboard/cashflow" },
  { id: "ai", label: "محلل الأرباح الذكي", icon: Brain, href: "/dashboard/ai" },
  { id: "profile", label: "الملف الشخصي", icon: UserRound, href: "/dashboard/profile" },
];

type Props = {
  user: AuthUser | null;
  onLogout: () => void;
};

export default function Sidebar({ user, onLogout }: Props) {
  const displayName = user ? `${user.firstName} ${user.lastName}`.trim() || user.email : "المستخدم";

  return (
    <aside className="fixed inset-y-0 right-0 z-40 hidden w-60 flex-col border-l border-white/5 bg-navy text-white/90 lg:flex">
      <div className="border-b border-white/5 p-5">
        <Link to="/" className="flex items-center gap-3">
          <div className="grid h-10 w-10 place-items-center rounded-xl bg-gradient-brand font-bold text-white shadow-glow">
            م
          </div>
          <div>
            <div className="text-lg font-bold leading-tight text-white">مالي</div>
            <div className="text-[11px] text-white/50">إدارة مالية ذكية</div>
          </div>
        </Link>
      </div>

      <nav className="flex-1 space-y-1 overflow-y-auto p-3">
        {items.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.id}
              to={item.href}
              end={item.href === "/dashboard"}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors ${
                  isActive
                    ? "bg-gradient-brand text-white shadow-glow"
                    : "text-white/70 hover:bg-white/5 hover:text-white"
                }`
              }
            >
              <Icon className="h-4 w-4" />
              <span>{item.label}</span>
            </NavLink>
          );
        })}
      </nav>

      <div className="border-t border-white/5 p-4">
        <div className="flex items-center gap-3">
          <div className="grid h-9 w-9 place-items-center rounded-full bg-gradient-brand font-bold">
            {displayName.charAt(0)}
          </div>
          <div className="min-w-0 flex-1 text-sm">
            <div className="truncate font-semibold text-white">{displayName}</div>
            <div className="truncate text-xs text-white/50">{user?.email ?? "حساب مستخدم"}</div>
          </div>
          <button
            type="button"
            onClick={onLogout}
            className="grid h-9 w-9 shrink-0 place-items-center rounded-xl text-white/60 transition-colors hover:bg-white/5 hover:text-white"
            aria-label="تسجيل الخروج"
            title="تسجيل الخروج"
          >
            <LogOut className="h-4 w-4" />
          </button>
        </div>
      </div>
    </aside>
  );
}
