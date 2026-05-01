import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ArrowLeft, Loader2, Mail } from "lucide-react";
import AuthLayout from "@/components/auth/AuthLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const FORGOT_PASSWORD_API_URL = `${API_BASE_URL}/api/v1/Auth/forgot-password`;

export default function ForgotPassword() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [timeLeft, setTimeLeft] = useState(0);

  useEffect(() => {
    if (timeLeft <= 0) return;
    const timer = setInterval(() => {
      setTimeLeft((prev) => prev - 1);
    }, 1000);
    return () => clearInterval(timer);
  }, [timeLeft]);

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, "0")}`;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError("");

    try {
      const response = await fetch(FORGOT_PASSWORD_API_URL, {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ email: email.trim() }),
      });

      const payload = await response.json().catch(() => null);

      if (!response.ok) {
        const msg = (payload as { message?: string; detail?: string; title?: string } | null)?.message ??
          (payload as { detail?: string; title?: string } | null)?.detail ??
          "تعذر إرسال رمز إعادة التعيين. تحقق من البريد الإلكتروني وحاول مرة أخرى.";
        setError(msg);
        if (msg.includes("5 دقائق")) {
          setTimeLeft(300);
        }
        return;
      }

      navigate(`/reset-password?email=${encodeURIComponent(email.trim())}`);
    } catch {
      setError("تعذر الاتصال بالخدمة. حاول مرة أخرى بعد قليل.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AuthLayout
      badge="استعادة الحساب"
      title="نسيت كلمة المرور؟"
      subtitle="أدخل بريدك الإلكتروني وسنرسل رمز تحقق لإعادة تعيين كلمة المرور."
      footer={
        <>
          تذكرت كلمة المرور؟{" "}
          <Link to="/login" className="text-teal font-semibold hover:underline">
            سجل دخولك
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

        <Button
          type="submit"
          size="lg"
          disabled={isSubmitting || timeLeft > 0}
          className="h-11 w-full rounded-xl bg-gradient-brand text-base shadow-glow hover:opacity-90"
        >
          {isSubmitting ? (
            <Loader2 className="ml-1 h-4 w-4 animate-spin" />
          ) : (
            <ArrowLeft className="ml-1 h-4 w-4" />
          )}
          {isSubmitting ? "جاري الإرسال..." : timeLeft > 0 ? `إرسال مجدداً خلال ${formatTime(timeLeft)}` : "إرسال رمز التحقق"}
        </Button>
      </form>
    </AuthLayout>
  );
}
