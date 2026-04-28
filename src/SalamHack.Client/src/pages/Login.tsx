import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ArrowLeft, Eye, EyeOff, Loader2, Lock, Mail } from "lucide-react";
import AuthLayout from "@/components/auth/AuthLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { storeAuthSession, type AuthSessionResponse } from "@/lib/auth";

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const LOGIN_API_URL = `${API_BASE_URL}/api/v1/Auth/login`;

type LoginForm = {
  email: string;
  password: string;
};

export default function Login() {
  const navigate = useNavigate();
  const [showPwd, setShowPwd] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [form, setForm] = useState<LoginForm>({
    email: "",
    password: "",
  });

  const setField = (field: keyof LoginForm) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [field]: e.target.value }));
    setError("");
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError("");

    try {
      const response = await fetch(LOGIN_API_URL, {
        method: "POST",
        credentials: "include",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          email: form.email.trim(),
          password: form.password,
        }),
      });

      const payload = await response.json().catch(() => null);

      if (!response.ok) {
        setError(
          (payload as { detail?: string; title?: string } | null)?.detail ??
            "تعذر تسجيل الدخول. تحقق من البريد الإلكتروني وكلمة المرور.",
        );
        return;
      }

      storeAuthSession(payload as AuthSessionResponse);
      navigate("/dashboard");
    } catch {
      setError("تعذر الاتصال بالخدمة. حاول مرة أخرى بعد قليل.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AuthLayout
      badge="تسجيل الدخول"
      title="أهلا بعودتك"
      subtitle="سجل دخولك للمتابعة إلى لوحة تحكمك المالية."
      footer={
        <>
          ليس لديك حساب؟{" "}
          <Link to="/register" className="text-teal font-semibold hover:underline">
            أنشئ حسابا جديدا
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-3.5">
        {error && (
          <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-xs leading-relaxed text-danger">
            {error}
          </div>
        )}

        <div className="space-y-2">
          <Label htmlFor="email" className="text-navy">
            البريد الإلكتروني
          </Label>
          <div className="relative">
            <Mail className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="email"
              type="email"
              value={form.email}
              onChange={setField("email")}
              placeholder="name@example.com"
              required
              className="h-11 rounded-xl border-border/70 bg-card pr-10"
            />
          </div>
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <Label htmlFor="password" className="text-navy">
              كلمة المرور
            </Label>
            <a href="#" className="text-xs text-teal hover:underline">
              نسيت كلمة المرور؟
            </a>
          </div>
          <div className="relative">
            <Lock className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="password"
              type={showPwd ? "text" : "password"}
              value={form.password}
              onChange={setField("password")}
              placeholder="••••••••"
              required
              className="h-11 rounded-xl border-border/70 bg-card pr-10 pl-10"
            />
            <button
              type="button"
              onClick={() => setShowPwd((s) => !s)}
              className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-navy"
              aria-label="إظهار كلمة المرور"
            >
              {showPwd ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>
        </div>

        <label className="flex select-none items-center gap-2 text-sm text-muted-foreground">
          <input type="checkbox" className="h-4 w-4 rounded border-border accent-teal" />
          تذكرني على هذا الجهاز
        </label>

        <Button
          type="submit"
          size="lg"
          disabled={isSubmitting}
          className="h-11 w-full rounded-xl bg-gradient-brand text-base shadow-glow hover:opacity-90"
        >
          {isSubmitting ? (
            <Loader2 className="ml-1 h-4 w-4 animate-spin" />
          ) : (
            <ArrowLeft className="ml-1 h-4 w-4" />
          )}
          {isSubmitting ? "جاري تسجيل الدخول..." : "دخول"}
        </Button>

        <div className="relative my-2">
          <div className="absolute inset-0 flex items-center">
            <span className="w-full border-t border-border" />
          </div>
          <div className="relative flex justify-center text-xs">
            <span className="bg-background px-3 text-muted-foreground">أو</span>
          </div>
        </div>

        <Button type="button" variant="outline" size="lg" className="h-11 w-full rounded-xl border-border/70">
          المتابعة عبر Google
        </Button>
      </form>
    </AuthLayout>
  );
}
