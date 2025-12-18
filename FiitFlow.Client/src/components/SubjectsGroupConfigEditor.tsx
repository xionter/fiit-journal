import { useNavigate } from 'react-router-dom'
import { useEffect, useState } from "react"
import * as yup from "yup"
import type Student from "./Student"
import api from "./Api"
import { rootMain } from "./Navigation"
import { useForm, useFieldArray, type UseFormRegister, type FieldErrors, type Control } from "react-hook-form";
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
    name: yup.string().required("Название предмета").matches(/^[А-ЯЁа-яё -\.]+$/, "Неверный формат"),
    link: yup.string().url("Неверный формат ссылки").required("Ссылка на таблицу").matches(googleSheetRegex, "Неверный формат"),
    formula: yup.string().required("Формула для подсчета баллов").matches(googleSheetRegex, "Неверный формат"),
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
            <form onSubmit={handleSubmit(data => onSubmit(data))} className="login-form">
                {subjectFields.map((subjectField, subjectIndex) => (
                    <SubjectEditor
                        key={subjectField.id}
                        subjectBaseName={subjectField.baseName}
                        subjectIndex={subjectIndex}
                        control={control}
                        register={register}
                        errors={errors}
                        onRemove={() => remove(subjectIndex)}
                    />
                ))}

                <button
                    type="button"
                    className="add-subject-btn"
                    onClick={() => append({ baseName: "", name: "", link: "", formula: "", sheets: [{ sheetName: "Sheet 1", headerRow: 1 }] })}
                > + </button>

                <button
                    type="submit"
                    className="login-btn"
                    disabled={!isValid || isSubmitting}
                >
                    {isSubmitting ? "Отправка..." : "Сохранить"}
                </button>

                {errors.root && (
                    <p className="text-red-500 text-sm">{errors.root.message}</p>
                )}
            </form>
        </LoadingPageData>
    );

    async function onSubmit(data: FormSubjects) {
        const isOk = await api.post(`ConfigEdit/SetConfigs`, data, {
            withCredentials: true,
            params: {
                firstName: student.firstName,
                lastName: student.lastName,
                group: student.group,
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
    subjectBaseName: string
    subjectIndex: number;
    control: Control<FormSubjects>;
    register: UseFormRegister<FormSubjects>;
    errors: FieldErrors<FormSubjects>;
    onRemove: () => void;
}

function SubjectEditor({ subjectBaseName, subjectIndex, control, register, errors, onRemove }: SubjectEditorProps) {
    const {
        fields: sheetFields,
        append: appendSheet,
        remove: removeSheet
    } = useFieldArray({
        control,
        name: `subjects.${subjectIndex}.sheets`
    });

    return (
        <div className="subject-card">
            <div className="subject-name">{subjectBaseName}</div>

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

            <div className="form-group">
                <label>Листы</label>
                {sheetFields.map((sheetField, sheetIndex) => {
                    const sheetError = errors.subjects?.[subjectIndex]?.sheets?.[sheetIndex];

                    return (
                        <div key={sheetField.id}>
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
                                    onClick={() => removeSheet(sheetIndex)}
                                >
                                    Удалить лист
                                </button>
                            )}
                        </div>
                    );
                })}
                <button
                    type="button"
                    onClick={() => appendSheet({ sheetName: "", headerRow: 1 })}
                >
                    Добавить лист
                </button>
            </div>

            <button
                type="button"
                className="login-btn secondary"
                onClick={onRemove}
            >
                Удалить
            </button>
        </div>
    );
}
