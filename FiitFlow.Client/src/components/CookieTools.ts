import Cookies from "js-cookie";
import type Student from "./Student";
import type StudentSubject from "./StudentSubject"

const studentCookieKey = "fiitflow-student";
const subjectCookieKey = "fiitflow-subject";

export function loadStudentCookie(): Student | undefined {
    const studentFromCookie = loadCookieJson(studentCookieKey);
    return studentFromCookie === null ? undefined : studentFromCookie;
}

export function loadSubjectCookie(): StudentSubject | undefined {
    const subjectFromCookie = loadCookieJson(subjectCookieKey);
    return subjectFromCookie === null ? undefined : subjectFromCookie;
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

export function saveSubjectCookie(subject: StudentSubject, expires: number) {
    Cookies.set(subjectCookieKey, JSON.stringify(subject), {
        expires: Math.abs(expires),
        path: "/",
        secure: true,
        sameSite: "strict"
    });
}

export function removeStudentCookie(expires: number) {
    Cookies.remove(studentCookieKey, {
        expires: Math.abs(expires),
        path: "/",
        secure: true,
        sameSite: "strict"
    });
}

export function removeSubjectCookie(expires: number) {
    Cookies.remove(subjectCookieKey, {
        expires: Math.abs(expires),
        path: "/",
        secure: true,
        sameSite: "strict"
    });
}