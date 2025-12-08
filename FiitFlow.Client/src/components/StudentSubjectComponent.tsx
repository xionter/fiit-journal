import type StudentSubject from "./StudentSubject"
import LoadingPageData from "./LoadingPageData"
import api from "./Api"

function StudentSubjectComponent({ subjectName, student, term }: StudentSubject) {
    return (
        <LoadingPageData isLoading={false}>{subjectName} {term} {student.secondName} {student.firstName}</LoadingPageData>
    );

    async function populateSubjectPointsDataByStudent() {
        api.get(`StudentSubjects/All`, {
            params: {
                id: student.id,
                firstName: student.firstName,
                secondName: student.secondName,
                group: student.group,
                term: term,
                time: Date.now(),
            }
        }).then(response => {
            if (response.status == 200) {
                const a = (response.data);
            }
        });
    }
}

export default StudentSubjectComponent;