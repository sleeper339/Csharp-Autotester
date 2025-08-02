CREATE DATABASE Autotester;
USE Autotester;

--********
--*Schema*
--********
create table tblTeacher(
teacherId int primary key identity,
teacherFullname varchar(80),
teacherEmail varchar(80),
teacherPasswordHash varchar(80)
);

create table tblFlatGrades(
fgId int primary key identity,
fgStudent int,
fgExam int,
fgGrade real
);

create table tblExam(
examId int primary key identity,
examStartTime datetime,
examEndTime datetime,
examAllotedTime int,
examinerTeacher int
);

create table tblStudent(
studentId int primary key identity,
studentFullname varchar(80),
studentEmail varchar(80),
studentPasswordHash varchar(80)
);

create table tblAnswers(
answerId int primary key identity,
answeredByStudent int,
answerForQuestion int,
answerOption int
);

create table tblRightAnswer(
rAnswerId int primary key identity,
rAnswerQuestion int,
rAnswerOption int
);

create table tblQuestion(
questionId int primary key identity,
questionContent text,
questionWeight real,
questionExam int
);

create table tblOption(
optionId int primary key identity,
optionContent text,
optionQuestion int
);

create table tblRegistration(
regId int primary key identity,
regStudent int,
regExam int
);

--*************
--*Constraints*
--*************

alter table tblAnswers
add constraint noDuplicate
unique (answeredByStudent, answerForQuestion);

alter table tblAnswers
add constraint fk_answerdBy
foreign key (answeredByStudent) references tblStudent(studentId);

alter table tblAnswers
add constraint fk_answerFor
foreign key (answerForQuestion) references tblQuestion(questionId);

alter table tblAnswers
add constraint fk_answerOption
foreign key (answerOption) references tblOption(optionId);

alter table tblRightAnswer
add constraint fk_rAnswerQuestion
foreign key (rAnswerQuestion) references tblQuestion(questionId);

alter table tblRightAnswer
add constraint fk_rAnswerOption
foreign key (rAnswerOption) references tblOption(optionId);

alter table tblRightAnswer
add constraint uq_rAnswer
unique (rAnswerQuestion);

alter table tblExam
add constraint fk_examiner
foreign key (examinerTeacher) references tblTeacher(teacherId);

alter table tblQuestion
add constraint fk_QuestionExam
foreign key (questionExam) references tblExam(examId);

alter table tblExam
add constraint ck_ValidStartEnd
check (examStartTime < examEndTime);

alter table tblRegistration
add constraint fk_regStudent
foreign key (regStudent) references tblStudent(studentId);

alter table tblRegistration
add constraint fk_regExam
foreign key (regExam) references tblExam(examId);

alter table tblRegistration
add constraint uq_Registration
unique (regStudent, regExam);

alter table tblOption
add constraint fk_OptionQuestion
foreign key (optionQuestion) references tblQuestion(questionId);

alter table tblQuestion
add constraint df_Weight
default(1) for questionWeight;

alter table tblStudent
add constraint ck_Email
check (studentEmail like '%@%.%');

alter table tblTeacher
add constraint ck_TeacherEmail
check (teacherEmail like '%@%.%');

--***********************
--*Programmable Elements*
--***********************