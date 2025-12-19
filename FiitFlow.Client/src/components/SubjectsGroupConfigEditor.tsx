import { useNavigate } from 'react-router-dom'
import { useEffect, useState } from "react"
import * as yup from "yup"
import type Student from "./Student"
import api from "./Api"
import { rootMain } from "./Navigation"
import { useForm, useFieldArray, type UseFormRegister, type FieldErrors, type Control, type UseFormTrigger } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";
import LoadingPageData from "./LoadingPageData"

const googleSheetRegex = /^https?:\/\/docs\.google\.com\/spreadsheets\/(?:u\/\d+\/)?d\/([a-zA-Z0-9-_]+)(?:\/[^\s]*)?$/;

interface ConfigEditorProps {
    student: Student;
    term: number;
}

interface SheetInput {
    sheetName: string;
    headerRow: number;
}

interface SubjectConfigInput {
    baseName: string;
    name: string;
    link: string;
    formula: string;
    sheets: SheetInput[];
}

interface FormSubjects {
    subjects: SubjectConfigInput[];
}

const sheetSchema = yup.object({
    sheetName: yup.string().required("Имя листа обязательно"),
    headerRow: yup.number()
        .typeError("Номер строки заголовка — число")
        .integer("Целое число")
        .min(1, "Минимум 1")
        .max(4, "Максимум 4")
        .required("Укажите номер строки заголовка"),
})

const subjectSchema = yup.object({
    baseName: yup.string().required(),
    name: yup.string().required("Название предмета"), //.matches(/^[А-ЯЁа-яё \_\-\.]+$/, "Неверный формат"),
    link: yup.string().url("Неверный формат ссылки").required("Ссылка на таблицу").matches(googleSheetRegex, "Неверный формат"),
    formula: yup.string().required("Формула для подсчета баллов"), //.matches(googleSheetRegex, "Неверный формат"),
    sheets: yup.array().of(sheetSchema).min(1, "Добавьте хотя бы один лист").required()
});

const schema = yup.object({
    subjects: yup.array().of(subjectSchema).min(1, "Данных вашей группы ещё нет в базе, ткните куру").required()
}).required();

export default function SubjectsGroupConfigEditor({ student, term }: ConfigEditorProps) {
    const navigate = useNavigate();
    const [baseSubCon, setBaseSubCon] = useState<SubjectConfigInput[]>();

    const {
        control,
        register,
        handleSubmit,
        setValue,
        setError,
        trigger,
        formState: { errors, isValid, isSubmitting }
    } = useForm<FormSubjects>({
        resolver: yupResolver(schema),
        mode: "onChange"
    });

    const { fields: subjectFields, append, remove } = useFieldArray({
        control,
        name: "subjects"
    });

    useEffect(() => {
        loadStudentConfigSubjects();
    }, []);

    useEffect(() => {
        if (baseSubCon !== undefined) {
            setValue("subjects", baseSubCon);
            trigger();
        }
    }, [baseSubCon]);

    return (
        <LoadingPageData isLoading={baseSubCon === undefined}>
            <div className="edit-container">
                <div className="edit-instructions">
                    <h3>📝 Инструкция по настройке</h3>
                    <ol>
                        <li>Укажите название предмета</li>
                        <li>Вставьте ссылку на Google таблицу с баллами</li>
                        <li>Укажите формулу для подсчета итогового балла</li>
                        <li>Настройте листы таблицы (имя листа и строку с заголовками)</li>
                        <li>Нажмите "Сохранить изменения"</li>
                    </ol>
                </div>
                <form onSubmit={handleSubmit(data => onSubmit(data))} className="login-form">
                    {subjectFields.map((subjectField, subjectIndex) => (
                        <div key={subjectField.id} className="subject-card edit-card">
                            <div className="subject-name">{subjectField.baseName}</div>

                            <div className="form-group">
                                <label>Предмет</label>
                                <input
                                    {...register(`subjects.${subjectIndex}.name`)}
                                    className={`input ${errors.subjects?.[subjectIndex]?.name ? 'input-error' : ''}`}
                                    placeholder="Введите название предмета"
                                />
                                {errors.subjects?.[subjectIndex]?.name && (
                                    <p className="text-red-500 text-sm">
                                        {errors.subjects[subjectIndex]!.name!.message}
                                    </p>
                                )}
                            </div>

                            <div className="form-group">
                                <label>Ссылка на таблицу</label>
                                <input
                                    {...register(`subjects.${subjectIndex}.link`)}
                                    className={`input ${errors.subjects?.[subjectIndex]?.link ? 'input-error' : ''}`}
                                    placeholder="Вставьте ссылку на таблицу"
                                />
                                {errors.subjects?.[subjectIndex]?.link && (
                                    <p className="text-red-500 text-sm">
                                        {errors.subjects[subjectIndex]!.link!.message}
                                    </p>
                                )}
                            </div>

                            <div className="form-group">
                                <label>Формула подсчета</label>
                                <input
                                    {...register(`subjects.${subjectIndex}.formula`)}
                                    className={`input ${errors.subjects?.[subjectIndex]?.formula ? 'input-error' : ''}`}
                                    placeholder="Введите формулу"
                                />
                                {errors.subjects?.[subjectIndex]?.formula && (
                                    <p className="text-red-500 text-sm">
                                        {errors.subjects[subjectIndex]!.formula!.message}
                                    </p>
                                )}
                            </div>

                            <SubjectSheetsEditor
                                key={subjectField.id}
                                subjectIndex={subjectIndex}
                                control={control}
                                register={register}
                                errors={errors}
                                trigger={trigger}
                            />

                            <button
                                type="button"
                                className="btn btn-danger"
                                onClick={() => { remove(subjectIndex); trigger(); }}
                            >
                                Удалить предмет
                            </button>
                        </div>
                    ))}

                    <div className="add-subject-header">
                        <button
                            type="button"
                            className="btn btn-primary add-subject-main-btn"
                            onClick={() => {
                                append({ baseName: "", name: "", link: "", formula: "", sheets: [{ sheetName: "Sheet 1", headerRow: 1 }] });
                                trigger();
                            }}
                        >
                            + Добавить новый предмет
                        </button>
                    </div>

                    <div className="save-section">
                        <div className="submit-section">
                            {errors.root && (
                                <p className="error-text global-error">{errors.root.message}</p>
                            )}
                            <button
                                type="submit"
                                className="btn btn-primary add-subject-main-btn"
                                disabled={!isValid || isSubmitting}
                            >
                                {isSubmitting ? "Отправка..." : "Сохранить"}
                            </button>
                        </div>
                    </div>
                </form>
            </div>
        </LoadingPageData>
    );

    async function onSubmit(data: FormSubjects) {
        const isOk = await api.post(`ConfigEdit/SetConfigs`, data.subjects, {
            withCredentials: true,
            params: {
                id: student.id,
                firstName: student.firstName,
                lastName: student.lastName,
                group: student.group,
                term: term,
                time: Date.now()
            }
        }).then(response => {
            if (response.status == 200)
                return response.data;
            return null;
        });
        if (isOk)
            navigate(rootMain.to, rootMain.options);
        else
            setError("root.serverError", {
                type: "server",
                message: "Ошибка"
            });
    }

    function loadStudentConfigSubjects() {
        api.get<SubjectConfigInput[]>("ConfigEdit/GetConfigs", {
            withCredentials: true,
            params: {
                id: student.id,
                firstName: student.firstName,
                lastName: student.lastName,
                group: student.group,
                term: term,
                time: Date.now(),
            }
        }).then(response => {
            if (response.status == 200) {
                setBaseSubCon(response.data);
            }
        });
        return baseSubCon;
    }
}

interface SubjectEditorProps {
    subjectIndex: number;
    control: Control<FormSubjects>;
    register: UseFormRegister<FormSubjects>;
    errors: FieldErrors<FormSubjects>;
    trigger: UseFormTrigger<FormSubjects>;
}

function SubjectSheetsEditor({ subjectIndex, control, register, errors, trigger }: SubjectEditorProps) {
    const {
        fields: sheetFields,
        append: appendSheet,
        remove: removeSheet
    } = useFieldArray({
        control,
        name: `subjects.${subjectIndex}.sheets`
    });

    return (
        <div className="form-group">
            <label>Листы</label>
            {sheetFields.map((sheetField, sheetIndex) => {
                const sheetError = errors.subjects?.[subjectIndex]?.sheets?.[sheetIndex];

                return (
                    <div key={sheetField.id} className="sheet-card">
                        <div className="form-group">
                            <label>Имя листа</label>
                            <input
                                {...register(
                                    `subjects.${subjectIndex}.sheets.${sheetIndex}.sheetName`
                                )}
                                className={`input ${sheetError?.sheetName ? "input-error" : ""
                                    }`}
                            />
                            {sheetError?.sheetName && (
                                <p className="error-text">
                                    {sheetError.sheetName.message}
                                </p>
                            )}
                        </div>

                        <div className="form-group">
                            <label>Строка заголовка</label>
                            <input
                                type="number"
                                {...register(
                                    `subjects.${subjectIndex}.sheets.${sheetIndex}.headerRow`,
                                    { valueAsNumber: true }
                                )}
                                className={`input ${sheetError?.headerRow ? "input-error" : ""
                                    }`}
                            />
                            {sheetError?.headerRow && (
                                <p className="error-text">
                                    {sheetError.headerRow.message}
                                </p>
                            )}
                        </div>

                        {sheetFields.length > 1 && (
                            <button
                                type="button"
                                className="btn btn-outline btn-sm"
                                onClick={() => { removeSheet(sheetIndex); trigger(); }}
                            >
                                Удалить лист
                            </button>
                        )}
                    </div>
                );
            })}
            <button
                type="button"
                className="btn btn-sm"
                onClick={() => { appendSheet({ sheetName: "", headerRow: 1 }); trigger(); }}
            >
                Добавить лист
            </button>
        </div>
    );
}
