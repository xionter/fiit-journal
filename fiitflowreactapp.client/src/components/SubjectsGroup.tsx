import Fragment from "react"
import "./SubjectsGroup.css"

function SubjectsGroup() {
    return (
        <div className="container">
            <h1 className="page-title">Мои предметы</h1>
            <div className="subjects-grid">
                <div className="subject-card">
                    <div className="subject-name">Алгебра и геометрия</div>
                    <div className="subject-score">95</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 15.05.2023</span>
                        <span>Преподаватель: Петрова А.С.</span>
                    </div>
                    <a href="algebra.html" className="btn">Подробнее</a>
                </div>

                <div className="subject-card">
                    <div className="subject-name">Язык Python</div>
                    <div className="subject-score">95</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 14.05.2023</span>
                        <span>Преподаватель: Сидоров В.П.</span>
                    </div>
                    <a href="python.html" className="btn">Подробнее</a>
                </div>

                <div className="subject-card">
                    <div className="subject-name">Проектный практикум</div>
                    <div className="subject-score">88</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 16.05.2023</span>
                        <span>Преподаватель: Козлов Д.М.</span>
                    </div>
                    <a href="project.html" className="btn">Подробнее</a>
                </div>

                <div className="subject-card">
                    <div className="subject-name">Математический анализ</div>
                    <div className="subject-score">92</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 12.05.2023</span>
                        <span>Преподаватель: Васильева О.И.</span>
                    </div>
                    <a href="math-analysis.html" className="btn">Подробнее</a>
                </div>

                <div className="subject-card">
                    <div className="subject-name">Основы программирования</div>
                    <div className="subject-score">80</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 10.05.2023</span>
                        <span>Преподаватель: Николаев П.С.</span>
                    </div>
                    <a href="programming.html" className="btn">Подробнее</a>
                </div>

                <div className="subject-card">
                    <div className="subject-name">Философия</div>
                    <div className="subject-score">98</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 11.05.2023</span>
                        <span>Преподаватель: Семенова Л.В.</span>
                    </div>
                    <a href="philosophy.html" className="btn">Подробнее</a>
                </div>

                <div className="subject-card">
                    <div className="subject-name">Английский язык</div>
                    <div className="subject-score">96</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 13.05.2023</span>
                        <span>Преподаватель: Brown J.</span>
                    </div>
                    <a href="english.html" className="btn">Подробнее</a>
                </div>

                <div className="subject-card">
                    <div className="subject-name">Дизайн</div>
                    <div className="subject-score">85</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 09.05.2023</span>
                        <span>Преподаватель: Орлова Е.К.</span>
                    </div>
                    <a href="design.html" className="btn">Подробнее</a>
                </div>

                <div className="subject-card">
                    <div className="subject-name">Физическая культура</div>
                    <div className="subject-score">87</div>
                    <div className="progress-bar">
                        <div className="progress-fill"></div>
                    </div>
                    <div className="progress-text">
                        <span>0</span>
                        <span>100</span>
                    </div>
                    <div className="subject-details">
                        <span>Последнее обновление: 08.05.2023</span>
                        <span>Преподаватель: Попов А.Н.</span>
                    </div>
                    <a href="pe.html" className="btn">Подробнее</a>
                </div>
            </div>
        </div>
    );
}

export default SubjectsGroup;