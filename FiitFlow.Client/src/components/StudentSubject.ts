import type Student from "./Student"

export default interface StudentSubject {
    subjectName: string;
    student: Student;
    term: number;
    score: number;
}