import { Fragment, useEffect, useState } from "react"
import Cookies from "js-cookie"
import type Student from "./Student";
import api from "./Api"

interface LoginProps {
    setCurrentStudent: Function;
}

function LoginPage({ setCurrentStudent }: LoginProps) {

    return (
        <p>Hello Student!<button onClick={() => saveCookie({ id: 1, firstName: "Пеганов", secondName: "Артем", group: "ФТ-201-2" })}>Set Name</button></p>
    );

    function loadCookie() {
        const studentDataString = Cookies.get("student");
        const studentData = studentDataString ? JSON.parse(studentDataString) : null;
    }

    function saveCookie({ id, firstName, secondName, group }: Student) {
        const data = {
            id: id,
            firstName: firstName,
            secondName: secondName,
            group: group
        };
        Cookies.set("student", JSON.stringify(data), {
            expires: 3,
            path: "/",
            secure: true,
            sameSite: "strict"
        });
        setCurrentStudent(data);
    }
}

export default LoginPage;