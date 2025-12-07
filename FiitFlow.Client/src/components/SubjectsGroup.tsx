import { Fragment, useEffect, useState } from "react"
import LoadingPageData from "./LoadingPageData"
import "./SubjectsGroup.css"
import type Student from "./Student"
import api from "./Api"

interface SubjectPoints {
    subject: string;
    teacher: string;
    score: number;
    lastUpdate: string;
}

function SubjectsGroup({ id, firstName, secondName, group }: Student) {
    const [points, setPoints] = useState<SubjectPoints[]>();

    useEffect(() => {
        populateSubjectPointsDataByStudent();
    }, []);

    return (
        <LoadingPageData isLoading={points === undefined}>
            <h1 className="page-title">Мои предметы</h1>
            <div className="subjects-grid">
                {
                    points?.map((subpoint) => (
                        <div className="subject-card" key={subpoint.subject}>
                            <div className="subject-name">{subpoint.subject}</div>
                            <div className="subject-score">{subpoint.score}</div>
                            <div className="progress-bar">
                                <div className="progress-fill"></div>
                            </div>
                            <div className="progress-text">
                                <span>0</span>
                                <span>100</span>
                            </div>
                            <div className="subject-details">
                                <span>Последнее обновление: {subpoint.lastUpdate}</span>
                                <span>Преподаватель: {subpoint.teacher}</span>
                            </div>
                            <a href={subpoint.subject} className="btn">Подробнее</a>
                        </div>
                    ))
                }
            </div>
        </LoadingPageData>
    );

    async function populateSubjectPointsDataByStudent() {
        api.get("StudentSubjects", {
            params: {
                id: id,
                firstName: firstName,
                secondName: secondName,
                group: group,
                time: Date.now(),
            }
        }).then(response => {
            if (response.status == 200) {
                setPoints(response.data);
            }
        });
    }
}

export default SubjectsGroup;