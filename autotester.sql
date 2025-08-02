--silly comment
create table tblTeacher(
teacherId serial primary key,
teacherFullname varchar(80),
teacherEmail varchar(80),
teacherPasswordHash varchar(80)
);

create table tblExam(
examId serial primary key,
startDate date,
startTime time,
endDate date,
endTime time,
examAllotedTime interval,
examinerTeacher int
);

create table tblStudent(
studentId serial primary key,
studentFullname varchar(80),
studentEmail varchar(80),
studentPasswordHash varchar(80)
);

create table tblAnswers(
answerId serial primary key,
answeredByStudent int,
answerForQuestion int,
answerOption int
);

create table tblRightAnswer(
rAnswerId serial primary key,
rAnswerQuestion int,
rAnswerOption int
);

--constraints for student's answers

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

--constraints for the correct answers

alter table tblRightAnswer
add constraint fk_rAnswerQuestion
foreign key (rAnswerQuestion) references tblQuestion(questionId);

alter table tblRightAnswer
add constraint fk_rAnswerOption
foreign key (rAnswerOption) references tblOption(optionId);

alter table tblRightAnswer
add constraint uq_rAnswer
unique (rAnswerQuestion);

--end of constraints

alter table tblExam
add constraint fk_examiner
foreign key (examinerTeacher) references tblTeacher(teacherId);

create table tblQuestion(
questionId serial primary key,
questionContent text,
questionWeight real,
questionExam int
);

alter table tblQuestion
add constraint fk_QuestionExam
foreign key (questionExam) references tblExam(examId);

alter table tblExam
add constraint ck_ValidStartEnd
check (startDate + startTime <= (endDate + endTime) - examAllotedTime);

create table tblOption(
optionId serial primary key,
optionContent text,
optionQuestion int
);

create table tblRegistration(
regId serial primary key,
regStudent int,
regExam int
);

alter table tblRegistration
add constraint fk_regStudent
foreign key (regStudent) references tblStudent(studentId);

alter table tblRegistration
add constraint fk_regExam
foreign key (regExam) references tblExam(examId);

alter table tblRegistration
add constraint uq_Registration
unique (regStudent, regExam);

create table tblFlatGrades(
fgId serial primary key,
fgStudent int,
fgExam int,
fgGrade real
);

alter table tblOption
add constraint fk_OptionQuestion
foreign key (optionQuestion) references tblQuestion(questionId);

alter table tblQuestion
alter column questionWeight
set default 1;

alter table tblStudent
add constraint ck_Email
check (studentEmail like '%@%.%');

alter table tblTeacher
add constraint ck_TeacherEmail
check (teacherEmail like '%@%.%');

-- ***********************
-- *Programmable Elements*
-- ***********************

create function getScoreForQuestion(qid int, sid int)
returns real as
$$
declare
    score real;
    selectedOption int;
    rightOption int;
begin
    select questionWeight into score from tblQuestion where questionId = qid;
    select answerOption into selectedOption from tblAnswers where answeredByStudent = sid and answerForQuestion = qid;
    select rAnswerOption into rightOption from tblRightAnswer where rAnswerQuestion = qid limit 1;
    if (selectedOption = rightOption) then
       return score;
    else
        return 0;
    end if;
end
$$
language plpgsql;

create function getScoreForExam(eid int, sid int, out score real) as
$$
begin
    select sum(getScoreForQuestion(questionId, sid)) into score from tblQuestion
    where questionExam = eid;
end
$$ language plpgsql;

--Dynamic responsive grade
create view vwDynamicGrades as select
regStudent as student, regExam as exam, getScoreForExam(regStudent, regExam) as grade
from tblRegistration;

create view vwAllGrades as
select * from vwDynamicGrades union
select fgStudent, fgExam, fgGrade from tblFlatGrades;
