import { BrowserRouter, Routes, Route, useNavigate } from "react-router-dom"
import SubjectsGroup from "./components/SubjectsGroup"
import StudentSubjectComponent from "./components/StudentSubjectComponent"
import LoginPage from "./components/LoginPage"
import Main from "./components/Main"
import "./fiitflow.css"
import { rootLogin, rootMain, rootEdit } from "./components/Navigation"

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<LoginPage />} />
                <Route path={rootLogin.to} element={<LoginPage />} />
                <Route path={rootMain.to} element={<Main subjectPeaked={false} isEditing={false} />} />
                <Route path={rootEdit.to} element={<Main subjectPeaked={false} isEditing={true} />} />
                <Route path={`${rootMain.to}/:subjectName`} element={<Main subjectPeaked={true} isEditing={false} />} />
            </Routes>
        </BrowserRouter>
    )
}

function ReAddress() {
    const navigate = useNavigate();
    return <div onLoadStart={() => navigate(rootLogin.to, rootLogin.options)}></div>;
}

export default App;