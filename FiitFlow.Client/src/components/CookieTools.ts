import Cookies from "js-cookie";
import type Student from "./Student";

const studentCookieKey = "fiitflow-student";

export function loadStudentCookie() {
    return loadCookieJson(studentCookieKey);
}

export function loadCookieJson(cookieName: string) {
    const DataString = Cookies.get(cookieName);
    return DataString ? JSON.parse(DataString) : null;
}

export function saveStudentCookie(student: Student, expires: number) {
    Cookies.set(studentCookieKey, JSON.stringify(student), {
        expires: Math.abs(expires),
        path: "/",
        secure: true,
        sameSite: "strict"
    });
}