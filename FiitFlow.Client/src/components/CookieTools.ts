import Cookies from "js-cookie";
import type Student from "./Student";
import type StudentSubject from "./StudentSubject";

const studentCookieKey = "fiitflow-student";
const subjectCookieKey = "fiitflow-subject";

const isHttps = typeof window !== "undefined" && window.location.protocol === "https:";

const cookieOpts = (expires: number) => ({
  expires: Math.abs(expires),
  path: "/",
  secure: isHttps,          // HTTP -> false, HTTPS -> true
  sameSite: "lax" as const, // strict часто мешает, lax норм
});

export function loadStudentCookie(): Student | undefined {
  const s = Cookies.get(studentCookieKey);
  if (!s) return undefined;
  try { return JSON.parse(s) as Student; } catch { return undefined; }
}

export function loadSubjectCookie(): StudentSubject | undefined {
  const s = Cookies.get(subjectCookieKey);
  if (!s) return undefined;
  try { return JSON.parse(s) as StudentSubject; } catch { return undefined; }
}

export function saveStudentCookie(student: Student, expires: number) {
  Cookies.set(studentCookieKey, JSON.stringify(student), cookieOpts(expires));
}

export function saveSubjectCookie(subject: StudentSubject, expires: number) {
  Cookies.set(subjectCookieKey, JSON.stringify(subject), cookieOpts(expires));
}

export function removeStudentCookie(expires: number) {
  Cookies.remove(studentCookieKey, cookieOpts(expires));
}

export function removeSubjectCookie(expires: number) {
  Cookies.remove(subjectCookieKey, cookieOpts(expires));
}

