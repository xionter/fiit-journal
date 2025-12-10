import { BrowserRouter, Routes, Route } from "react-router-dom"
import SubjectsGroup from "./components/SubjectsGroup"
import StudentSubjectComponent from "./components/StudentSubjectComponent"
import LoginPage from "./components/LoginPage"
import Main from "./components/Main"
import "./fiitflow.css"

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<LoginPage />} />
                <Route path="/Login" element={<LoginPage />} />
                <Route path="/StudentSubjects" element={<Main />} />
            </Routes>
        </BrowserRouter>
    )
}

export default App;