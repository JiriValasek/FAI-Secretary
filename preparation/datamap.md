# Classes (Tables)

## Subject

Describes a subject managed by a departement. 
Relations will be solved using a table (Subject <1==N> Employee,l,s,p), (Program <N==M> Subject),
(StudyGroup <N==M> Subject).

### Attributes (Columns)

* subjectId: integer;
* abbreviation: String;
* title:String;
* credites: integer;
* language: Enum { "en", "cs" }
* studyType: Enum { "combined", "presentiona" }
* year: String or Date;
* semester: Enum { "spring", "fall" }
* lectureCount: integer;
* seminarCount: integer;
* practiceTeacher: integer;
* conditions: Enum { "zápočet", "zápočet & zkouška", "klasifikovaný zápočet"};

### Methods

* todo

## Employee

Describes an employee of a departement.

### Attributes (Columns)

* titlesBefore: String[];
* titlesAfter: String[];
* name: String;
* surname: String;
* workPhone: String;
* mobilePhone: String;
* workEmail: String; 
* office: String;
* workLoad: float; <1
* doctorate: boolean;

### Methods

* checkValidEmail

## EmployeeWorkList

