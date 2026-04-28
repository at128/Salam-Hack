export const MONTHS = ["أكتوبر", "نوفمبر", "ديسمبر", "يناير", "فبراير", "مارس"];
export const REVENUE_DATA = [18500, 22000, 19800, 25000, 23500, 28000];
export const EXPENSE_DATA = [8200, 9500, 11000, 9800, 10200, 11500];

import {
  Wallet,
  TrendingUp,
  FileText,
  Calculator,
  Clock,
  BarChart3,
  CheckCircle2,
  Lightbulb,
  Bot,
  Banknote,
  BellRing,
  type LucideIcon,
} from "lucide-react";

export const KPIS: { label: string; value: string; delta: string; positive: boolean; icon: LucideIcon }[] = [
  { label: "إيراد هذا الشهر", value: "28,000 ر.س", delta: "+19٪", positive: true, icon: Wallet },
  { label: "صافي الربح", value: "16,500 ر.س", delta: "+22٪", positive: true, icon: TrendingUp },
  { label: "فواتير قيد التحصيل", value: "4 فواتير", delta: "9,920 ر.س", positive: false, icon: FileText },
  { label: "مصاريف الشهر", value: "11,500 ر.س", delta: "+12٪", positive: false, icon: Calculator },
];

export type Tx = {
  id: number;
  name: string;
  amount: number;
  type: "income" | "expense";
  date: string;
  status?: string;
};

export const TRANSACTIONS: Tx[] = [
  { id: 1, name: "فاتورة #207 — شركة نماء", amount: 4500, type: "income", date: "٢٥ مارس", status: "مدفوعة" },
  { id: 2, name: "اشتراك Figma السنوي", amount: -1200, type: "expense", date: "٢٣ مارس" },
  { id: 3, name: "فاتورة #206 — مؤسسة أفق", amount: 7800, type: "income", date: "٢٠ مارس", status: "مدفوعة" },
  { id: 4, name: "استضافة سيرفرات", amount: -350, type: "expense", date: "١٨ مارس" },
  { id: 5, name: "فاتورة #204 — عميل حر خالد", amount: 3200, type: "income", date: "١٠ مارس", status: "متأخرة" },
  { id: 6, name: "إعلان انستقرام", amount: -800, type: "expense", date: "١٥ مارس" },
];

export type AlertType = "warning" | "info" | "success";
export const ALERTS: { icon: LucideIcon; text: string; type: AlertType; action?: string }[] = [
  { icon: Clock, text: "فاتورة #204 متأخرة ١٥ يوم — تواصل مع العميل", type: "warning", action: "أرسل تذكير" },
  { icon: BarChart3, text: "مصاريفك زادت ١٢٪ عن الشهر الماضي", type: "info", action: "عرض المصاريف" },
  { icon: CheckCircle2, text: "فاتورة #207 تم تحصيلها بنجاح", type: "success" },
];

export const INVOICES = [
  { id: 207, client: "شركة نماء", service: "تصميم واجهات تطبيق", total: 5175, date: "٢٥ مارس ٢٠٢٦", status: "مدفوعة" },
  { id: 206, client: "مؤسسة أفق", service: "هوية بصرية كاملة", total: 8970, date: "٢٠ مارس ٢٠٢٦", status: "مدفوعة" },
  { id: 205, client: "شركة رؤية", service: "تصميم لاندنق بيج", total: 4025, date: "١٥ مارس ٢٠٢٦", status: "مرسلة" },
  { id: 204, client: "عميل حر — خالد", service: "تصميم شعار + بزنس كارد", total: 3680, date: "١٠ مارس ٢٠٢٦", status: "متأخرة" },
  { id: 203, client: "متجر سلّة", service: "بانرات إعلانية", total: 3220, date: "٥ مارس ٢٠٢٦", status: "مدفوعة" },
];

export const SERVICE_PROFITS = [
  { name: "تصميم واجهات UI/UX", profit: 28000, margin: 67 },
  { name: "هوية بصرية", profit: 13300, margin: 72 },
  { name: "استشارات UX", profit: 9200, margin: 77 },
  { name: "بانرات إعلانية", profit: 4200, margin: 50 },
  { name: "تصميم شعارات", profit: 1700, margin: 25 },
];

export const FEATURES: { icon: LucideIcon; title: string; desc: string }[] = [
  {
    icon: FileText,
    title: "فواتير احترافية بثوانٍ",
    desc: "أنشئ فواتير بضريبة القيمة المضافة، أرسلها بالبريد، وتابع حالتها لحظة بلحظة.",
  },
  {
    icon: Lightbulb,
    title: "تسعير ذكي مبني على بياناتك",
    desc: "اقتراحات أسعار حقيقية تعتمد على ساعاتك الفعلية وهامش ربحك في مشاريع مشابهة.",
  },
  {
    icon: BarChart3,
    title: "كشف الربح الحقيقي",
    desc: "ليس مجرد إيراد — احسب ربحك الصافي بعد المصاريف، الأدوات، والاشتراكات.",
  },
  {
    icon: Bot,
    title: "محلل ذكي لمشاريعك",
    desc: "تنبيهات فورية: أي مشروع يأكل من ربحك، وأي خدمة تستحق التوسع فيها.",
  },
  {
    icon: Banknote,
    title: "تدفق نقدي واضح",
    desc: "اعرف متى يصل المال ومتى يخرج، وخطط بثقة لشهرك القادم.",
  },
  {
    icon: BellRing,
    title: "تذكيرات تحصيل تلقائية",
    desc: "لا تنسَ فاتورة متأخرة — مالي يذكّر عملاءك باحترام نيابةً عنك.",
  },
];

export const TESTIMONIALS = [
  {
    name: "نورة العتيبي",
    role: "مصممة UI/UX مستقلة",
    quote: "لأول مرة أعرف ربحي الحقيقي بعد كل المصاريف. مالي غيّر طريقة تسعيري كلياً.",
  },
  {
    name: "خالد الشمري",
    role: "مطور ويب",
    quote: "الفواتير صارت تنجز في دقيقة، والتذكيرات التلقائية وفّرت علي محادثات محرجة.",
  },
  {
    name: "ريم الدوسري",
    role: "مستشارة تسويق",
    quote: "محلل الأرباح كشف لي مشاريع كنت أظنها رابحة. التطبيق يستحق كل ريال.",
  },
];

export const PRICING = [
  {
    name: "المبتدئ",
    price: "0",
    period: "مجاناً للأبد",
    features: ["حتى ٥ فواتير شهرياً", "إدارة عملاء أساسية", "تقارير شهرية"],
    cta: "ابدأ مجاناً",
    highlighted: false,
  },
  {
    name: "المحترف",
    price: "49",
    period: "ر.س / شهرياً",
    features: [
      "فواتير غير محدودة",
      "تسعير ذكي + محلل أرباح",
      "تذكيرات تحصيل تلقائية",
      "تقارير ضريبية جاهزة",
    ],
    cta: "ابدأ تجربة ١٤ يوم",
    highlighted: true,
  },
  {
    name: "الوكالة",
    price: "149",
    period: "ر.س / شهرياً",
    features: ["كل ميزات المحترف", "حتى ٥ مستخدمين", "دعم أولوية", "API مفتوح"],
    cta: "تواصل معنا",
    highlighted: false,
  },
];