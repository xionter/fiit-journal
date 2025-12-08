import { useEffect, useState, Fragment, type ReactElement } from 'react';
import SubjectsGroup from "./components/SubjectsGroup";
import StudentSubjectComponent from "./components/StudentSubjectComponent"
import LoginPage from './components/LoginPage';
import './fiitflow.css';
import type Student from "./components/Student";
import type StudentSubject from "./components/StudentSubject";

function App() {
    const [currentStudent, setCurrentStudent] = useState<Student>();
    const [currentSubject, setCurrentSubject] = useState<StudentSubject>();
    const [currentTerm, setCurrentTerm] = useState<number>(1);
    const [bodyBlock, setBodyBlock] = useState<ReactElement>();

    useEffect(() => {
        if (currentStudent === undefined)
            setBodyBlock(<LoginPage setCurrentStudent={setCurrentStudent} />);
        else if (currentSubject === undefined)
            setBodyBlock(mainBodyBlock(
                <SubjectsGroup
                    setSubject={setCurrentSubject}
                    student={currentStudent}
                    term={currentTerm}
                />
            ));
        else
            setBodyBlock(mainBodyBlock(
                <StudentSubjectComponent
                    subjectName={currentSubject.subjectName}
                    student={currentSubject.student}
                    term={currentTerm}
                />
            ));
    }, [currentStudent, currentSubject, currentTerm]);

    if (bodyBlock === undefined)
        return <h1>Block define error</h1>
    return bodyBlock;

    function mainBodyBlock(centralBlock: ReactElement) {
        return (
            <Fragment>
                <header>
                    <div className="div-container">
                        <div className="header-content">
                            <div className="logo">
                                <span className="logo-icon">📊</span>
                                FIITFLOW
                            </div>
                            <div className="semester-select">
                                <select onChange={(event) => setCurrentTerm(Number(event.target.value))} className="semester-dropdown">
                                    {
                                        [1, 2, 3, 4, 5, 6, 7, 8].map(num => (
                                            <option key={num} value={num}>Семестр {num}</option>
                                        ))
                                    }
                                </select>
                            </div>
                            <div className="user-info">
                                <div className="user-avatar">ФИИТ</div>
                                <span>{currentStudent?.lastName} {currentStudent?.firstName}</span>
                                <button onClick={logoutReset} className="logout-btn">Выход</button>
                            </div>
                        </div>
                    </div>
                </header>
                <div className="central-container">
                    {centralBlock}
                </div>
                <footer>
                    <div className="container">
                        <div className="footer-content">
                            <div className="footer-section">
                                <h3>FIITFLOW</h3>
                                <p>Единая система для отслеживания учебных баллов студентов ФИИТ</p>
                            </div>
                            <div className="footer-section">
                                <h3>Навигация</h3>
                                <ul>
                                    <li><a href="index.html">Главная</a></li>
                                    <li><a href="subjects.html">Предметы</a></li>
                                    <li><a href="analytics.html">Аналитика</a></li>
                                    <li><a href="settings.html">Настройки</a></li>
                                </ul>
                            </div>
                            <div className="footer-section">
                                <h3>Контакты</h3>
                                <ul>
                                    <li>Email: support@fiitflow.ru</li>
                                    <li>Телеграм: @fiitflow_support</li>
                                    <li>Кампус: УрФУ, корпус ФИИТ</li>
                                </ul>
                            </div>
                        </div>
                        <div className="copyright">
                            &copy; 2025 FIITFLOW. Все права защищены.
                        </div>
                    </div>
                </footer>
            </Fragment>
        );
    }

    function logoutReset() {
        setCurrentStudent(undefined);
        setCurrentSubject(undefined);
        setCurrentTerm(1);
    }
}

export default App;