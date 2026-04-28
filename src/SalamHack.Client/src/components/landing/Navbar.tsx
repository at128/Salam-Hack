import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

const links = [
  { href: "#features", label: "المميزات" },
  { href: "#how", label: "كيف يعمل" },
  { href: "#testimonials", label: "آراء العملاء" },
  { href: "#pricing", label: "الأسعار" },
];

export default function Navbar() {
  return (
    <header className="sticky top-0 z-50 bg-background/95 border-b border-border">
      <nav className="container flex items-center justify-between h-16">
        <Link to="/" className="flex items-center" aria-label="مالي">
          <span className="brand-wordmark text-2xl leading-none text-navy">مالي</span>
        </Link>

        <ul className="hidden md:flex items-center gap-8 text-sm font-medium text-muted-foreground">
          {links.map((l) => (
            <li key={l.href}>
              <a href={l.href} className="hover:text-navy transition-colors">
                {l.label}
              </a>
            </li>
          ))}
        </ul>

        <div className="flex items-center gap-2">
          <Link to="/login" className="hidden sm:inline-flex">
            <Button variant="ghost" size="sm">دخول</Button>
          </Link>
          <Link to="/register">
            <Button size="sm" className="rounded-lg bg-navy text-white hover:bg-navy-light shadow-glow">
              ابدأ مجاناً
            </Button>
          </Link>
        </div>
      </nav>
    </header>
  );
}
