import { Link, NavLink } from "react-router-dom";
import {
  BriefcaseBusiness,
  FileText,
  FolderKanban,
  LayoutDashboard,
  LogOut,
  ReceiptText,
  ShieldAlert,
  Users,
  type LucideIcon,
} from "lucide-react";
import type { AuthUser } from "@/lib/auth";
import { cn } from "@/lib/utils";

type NavItem = {
  id: string;
  label: string;
  icon: LucideIcon;
  href: string;
};

const navGroups: { label: string; items: NavItem[] }[] = [
  {
    label: "نظرة عامة",
    items: [
      { id: "dashboard", label: "لوحة التحكم", icon: LayoutDashboard, href: "/dashboard" },
    ],
  },
  {
    label: "إدارة العمل",
    items: [
      { id: "projects", label: "المشاريع", icon: FolderKanban, href: "/dashboard/projects" },
      { id: "customers", label: "العملاء", icon: Users, href: "/dashboard/customers" },
      { id: "services", label: "الخدمات والأسعار", icon: BriefcaseBusiness, href: "/dashboard/services" },
    ],
  },
  {
    label: "المال",
    items: [
      { id: "invoices", label: "الفواتير والتحصيل", icon: FileText, href: "/dashboard/invoices" },
      { id: "expenses", label: "المصاريف", icon: ReceiptText, href: "/dashboard/expenses" },
    ],
  },
  {
    label: "قبل الاتفاق",
    items: [
      { id: "client-risk", label: "تحليل العميل", icon: ShieldAlert, href: "/dashboard/client-risk" },
    ],
  },
];

type Props = {
  user: AuthUser | null;
  onLogout: () => void;
};

export default function Sidebar({ user, onLogout }: Props) {
  const displayName = user ? `${user.firstName} ${user.lastName}`.trim() || user.email : "المستخدم";

  return (
    <aside
      dir="rtl"
      className="fixed inset-y-0 right-0 z-40 hidden w-60 flex-col overflow-hidden border-l border-white/5 bg-navy text-white/90 lg:flex"
    >
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

      <div className="relative min-h-0 flex-1">
        {/* Scroll cues (modern, subtle) */}
        <div className="pointer-events-none absolute inset-x-0 top-0 z-10 h-6 bg-gradient-to-b from-navy/95 to-transparent" />
        <div className="pointer-events-none absolute inset-x-0 bottom-0 z-10 h-8 bg-gradient-to-t from-navy/95 to-transparent" />

        <nav className="sidebar-scroll h-full overflow-y-auto overscroll-contain p-3 pe-2 ps-3 [scrollbar-gutter:stable]">
          {navGroups.map((group) => (
            <div key={group.label} className="mb-5 last:mb-0">
              <div className="mb-2 px-3 text-[11px] font-semibold text-white/35">{group.label}</div>
              <div className="space-y-1">
                {group.items.map((item) => {
                  const Icon = item.icon;
                  return (
                    <NavLink
                      key={item.id}
                      to={item.href}
                      end={item.href === "/dashboard"}
                      className={({ isActive }) =>
                        cn(
                          "flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors outline-none",
                          "focus-visible:ring-2 focus-visible:ring-white/30 focus-visible:ring-offset-2 focus-visible:ring-offset-navy",
                          isActive
                            ? "bg-white text-navy shadow-glow"
                            : "text-white/70 hover:bg-white/5 hover:text-white",
                        )
                      }
                    >
                      <Icon className="h-4 w-4" />
                      <span>{item.label}</span>
                    </NavLink>
                  );
                })}
              </div>
            </div>
          ))}
        </nav>
      </div>

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
