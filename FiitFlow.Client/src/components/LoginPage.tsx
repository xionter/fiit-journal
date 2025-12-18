import { useNavigate } from 'react-router-dom'
import { Fragment, useEffect, useState } from "react"
import { useForm } from "react-hook-form"
import { yupResolver } from "@hookform/resolvers/yup"
import * as yup from "yup"
import type Student from "./Student"
import api from "./Api"
import { saveStudentCookie, loadStudentCookie } from "./CookieTools"
import { rootMain } from "./Navigation"

interface FormInputs {
    firstName: string;
    lastName: string;
    group: string;
}

const schema = yup.object({
    lastName: yup.string().required("Введите фамилию").matches(/^[А-ЯЁ][а-яё]*$/, "Неверный формат"),
    firstName: yup.string().required("Введите имя").matches(/^[А-ЯЁ][а-яё]*$/, "Неверный формат"),
    group: yup.string().required("Введите группу").matches(/^ФТ-\d\d\d-\d$/, "Неверный формат")
}).required();

export default function LoginPage() {
    const navigate = useNavigate();
    const {
        register,
        handleSubmit,
        setValue,
        setError,
        formState: { errors, isValid, isSubmitting }
    } = useForm<FormInputs>({
        resolver: yupResolver(schema),
        mode: "onChange"
    });

    useEffect(() => {
        const studentFromCookie = loadStudentCookie();
        if (studentFromCookie !== undefined) {
            setValue("lastName", studentFromCookie.lastName);
            setValue("firstName", studentFromCookie.firstName);
            setValue("group", studentFromCookie.group);
        }
    }, [])

    return (
        <div className="login-container">
            <div className="login-box">
                <div className="login-logo">
                    <h1>
                        <span className="logo-icon">📊</span>
                        FIITFLOW
                    </h1>
                    <p>Вход в систему</p>
                </div>
                <form onSubmit={handleSubmit(data => setStudentLoadMain(data))} className="login-form">
                    <div className="form-group">
                        <label htmlFor="lastName">Фамилия</label>
                        <input {...register("lastName")} className={`input ${errors.firstName ? 'input-error' : ''}`} placeholder="Введите вашу фамилию" />
                        {errors.lastName && <p className="text-red-500 text-sm">{errors.lastName.message}</p>}
                    </div>
                    <div className="form-group">
                        <label htmlFor="firstName">Имя</label>
                        <input {...register("firstName")} className={`input ${errors.firstName ? 'input-error' : ''}`} placeholder="Введите ваше имя" />
                        {errors.firstName && <p className="text-red-500 text-sm">{errors.firstName.message}</p>}
                    </div>
                    <div className="form-group">
                        <label htmlFor="group">Группа</label>
                        <input {...register("group")} className={`input ${errors.firstName ? 'input-error' : ''}`} placeholder="Например: ФТ-201-1" />
                        {errors.group && <p className="text-red-500 text-sm">{errors.group.message}</p>}
                    </div>
                    <button type="submit" className="login-btn" disabled={!isValid || isSubmitting}>{isSubmitting ? "Поиск студента" : "Войти"}</button>
                    {errors.root && <p className="text-red-500 text-sm">{errors.root.message}</p>}
                </form>
                <div className="login-info">
                    <p>Для входа используйте реальные данные</p>
                    <p>Система предназначена для студентов ФИИТ УрФУ</p>
                </div>
            </div>
        </div>
    );

    async function setStudentLoadMain(studentLogin: FormInputs) {
        const newId = await checkStudentLogin(studentLogin);
        if (newId === undefined)
            setError("root.serverError", {
                type: "server",
                message: "Студент не найден"
            });
        else {
            const student = {
                id: Number(newId),
                firstName: studentLogin.firstName,
                lastName: studentLogin.lastName,
                group: studentLogin.group
            };
            saveStudentCookie(student, 5);
            navigate(rootMain.to, rootMain.options);
        }
    }

    async function checkStudentLogin(studentLogin: FormInputs) {
        return api.post<number>(`Auth/login`, {
            firstName: studentLogin.firstName,
            lastName: studentLogin.lastName,
            group: studentLogin.group,
            time: Date.now()
        }, {
            withCredentials: true
        }).then(response => {
            if (response.status == 200)
                return response.data;
            return null;
        });
    }
}