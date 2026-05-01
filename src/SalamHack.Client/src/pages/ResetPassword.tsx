import { useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { ArrowLeft, Eye, EyeOff, Loader2, Lock, Mail, ShieldCheck } from "lucide-react";
import AuthLayout from "@/components/auth/AuthLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const RESET_PASSWORD_API_URL = `${API_BASE_URL}/api/v1/Auth/reset-password`;

export default function ResetPassword() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialEmail = useMemo(() => searchParams.get("email") ?? "", [searchParams]);
  const [email, setEmail] = useState(initialEmail);
  const [otp, setOtp] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [showPwd, setShowPwd] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError("");

    try {
      const response = await fetch(RESET_PASSWORD_API_URL, {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          email: email.trim(),
          otp,
          newPassword,
        }),
      });

      const payload = await response.json().catch(() => null);

      if (!response.ok) {
        setError(
          (payload as { message?: string; detail?: string; title?: string } | null)?.message ??
            (payload as { detail?: string; title?: string } | null)?.detail ??
            "تعذر إعادة تعيين كلمة المرور. تحقق من الرمز وكلمة المرور الجديدة.",
        );
        return;
      }

      navigate("/login");
    } catch {
      setError("تعذر الاتصال بالخدمة. حاول مرة أخرى بعد قليل.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AuthLayout
      badge="رمز التحقق"
      title="إعادة تعيين كلمة المرور"
      subtitle="أدخل الرمز المرسل إلى بريدك الإلكتروني واختر كلمة مرور جديدة."
      footer={
        <>
          لم يصلك الرمز؟{" "}
          <Link to="/forgot-password" className="text-teal font-semibold hover:underline">
            أرسل رمزاً جديداً
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
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                setError("");
              }}
              placeholder="name@example.com"
              required
              className="h-11 rounded-xl border-border/70 bg-card pr-10"
            />
          </div>
        </div>

        <div className="space-y-2">
          <Label htmlFor="otp" className="text-navy">
            رمز التحقق
          </Label>
          <div className="relative">
            <ShieldCheck className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="otp"
              type="text"
              inputMode="numeric"
              value={otp}
              onChange={(e) => {
                setOtp(e.target.value.replace(/\D/g, "").slice(0, 6));
                setError("");
              }}
              placeholder="123456"
              required
              className="h-11 rounded-xl border-border/70 bg-card pr-10 tracking-[0.35em]"
            />
          </div>
        </div>

        <div className="space-y-2">
          <Label htmlFor="newPassword" className="text-navy">
            كلمة المرور الجديدة
          </Label>
          <div className="relative">
            <Lock className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="newPassword"
              type={showPwd ? "text" : "password"}
              value={newPassword}
              onChange={(e) => {
                setNewPassword(e.target.value);
                setError("");
              }}
              placeholder="8 أحرف على الأقل"
              required
              minLength={8}
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

        <Button
          type="submit"
          size="lg"
          disabled={isSubmitting || otp.length !== 6}
          className="h-11 w-full rounded-xl bg-gradient-brand text-base shadow-glow hover:opacity-90"
        >
          {isSubmitting ? (
            <Loader2 className="ml-1 h-4 w-4 animate-spin" />
          ) : (
            <ArrowLeft className="ml-1 h-4 w-4" />
          )}
          {isSubmitting ? "جاري الحفظ..." : "تحديث كلمة المرور"}
        </Button>
      </form>
    </AuthLayout>
  );
}
