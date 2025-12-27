using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartSchoolAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_programs",
                columns: table => new
                {
                    academic_program_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_registration_open = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_academic_programs", x => x.academic_program_id);
                });

            migrationBuilder.CreateTable(
                name: "archived_classrooms",
                columns: table => new
                {
                    archived_classroom_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    original_classroom_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    course_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    program_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    teacher_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_archived_classrooms", x => x.archived_classroom_id);
                });

            migrationBuilder.CreateTable(
                name: "payment_settings",
                columns: table => new
                {
                    settings_id = table.Column<int>(type: "integer", nullable: false),
                    admin_full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_settings", x => x.settings_id);
                });

            migrationBuilder.CreateTable(
                name: "failed_students",
                columns: table => new
                {
                    failure_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_user_id = table.Column<int>(type: "integer", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    program_name_at_failure = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    failure_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    final_gpa = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    academic_program_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_failed_students", x => x.failure_id);
                    table.ForeignKey(
                        name: "fk_failed_students_academic_programs_academic_program_id",
                        column: x => x.academic_program_id,
                        principalTable: "academic_programs",
                        principalColumn: "academic_program_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "graduations",
                columns: table => new
                {
                    graduation_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_user_id = table.Column<int>(type: "integer", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    program_name_at_graduation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    graduation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    final_gpa = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    academic_program_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_graduations", x => x.graduation_id);
                    table.ForeignKey(
                        name: "fk_graduations_academic_programs_academic_program_id",
                        column: x => x.academic_program_id,
                        principalTable: "academic_programs",
                        principalColumn: "academic_program_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "program_registrations",
                columns: table => new
                {
                    registration_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    academic_program_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_receipt_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    payment_receipt_public_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    admin_notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_program_registrations", x => x.registration_id);
                    table.ForeignKey(
                        name: "fk_program_registrations_academic_programs_academic_program_id",
                        column: x => x.academic_program_id,
                        principalTable: "academic_programs",
                        principalColumn: "academic_program_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    academic_program_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_users_academic_programs_academic_program_id",
                        column: x => x.academic_program_id,
                        principalTable: "academic_programs",
                        principalColumn: "academic_program_id");
                });

            migrationBuilder.CreateTable(
                name: "archived_enrollments",
                columns: table => new
                {
                    archived_enrollment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    archived_classroom_id = table.Column<int>(type: "integer", nullable: false),
                    student_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    student_national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    practical_grade = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    exam_grade = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    final_grade = table.Column<decimal>(type: "numeric(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_archived_enrollments", x => x.archived_enrollment_id);
                    table.ForeignKey(
                        name: "fk_archived_enrollments_archived_classrooms_archived_classroom",
                        column: x => x.archived_classroom_id,
                        principalTable: "archived_classrooms",
                        principalColumn: "archived_classroom_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "graduation_certificates",
                columns: table => new
                {
                    graduation_id = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    certificate_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    public_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_graduation_certificates", x => x.graduation_id);
                    table.ForeignKey(
                        name: "fk_graduation_certificates_graduations_graduation_id",
                        column: x => x.graduation_id,
                        principalTable: "graduations",
                        principalColumn: "graduation_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_conversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_conversations", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_conversations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    academic_program_id = table.Column<int>(type: "integer", nullable: false),
                    coordinator_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.course_id);
                    table.ForeignKey(
                        name: "fk_courses_academic_programs_academic_program_id",
                        column: x => x.academic_program_id,
                        principalTable: "academic_programs",
                        principalColumn: "academic_program_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_courses_users_coordinator_id",
                        column: x => x.coordinator_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    reset_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_reset_tokens", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_password_reset_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    conversation_id = table.Column<int>(type: "integer", nullable: false),
                    sender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_messages_chat_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "chat_conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "classrooms",
                columns: table => new
                {
                    classroom_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    teacher_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_classrooms", x => x.classroom_id);
                    table.ForeignKey(
                        name: "fk_classrooms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_classrooms_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    question_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    image_public_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    question_type = table.Column<string>(type: "varchar(50)", nullable: false),
                    difficulty_level = table.Column<string>(type: "varchar(50)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    reviewed_by_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_questions", x => x.question_id);
                    table.ForeignKey(
                        name: "fk_questions_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_questions_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_questions_users_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "weekly_challenge_submissions",
                columns: table => new
                {
                    weekly_challenge_submission_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    year = table.Column<int>(type: "integer", nullable: false),
                    week_of_year = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    time_taken_seconds = table.Column<int>(type: "integer", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    course_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_weekly_challenge_submissions", x => x.weekly_challenge_submission_id);
                    table.ForeignKey(
                        name: "fk_weekly_challenge_submissions_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_weekly_challenge_submissions_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "announcements",
                columns: table => new
                {
                    announcement_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    posted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    target_scope = table.Column<string>(type: "text", nullable: false),
                    academic_program_id = table.Column<int>(type: "integer", nullable: true),
                    course_id = table.Column<int>(type: "integer", nullable: true),
                    classroom_id = table.Column<int>(type: "integer", nullable: true),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_announcements", x => x.announcement_id);
                    table.ForeignKey(
                        name: "fk_announcements_academic_programs_academic_program_id",
                        column: x => x.academic_program_id,
                        principalTable: "academic_programs",
                        principalColumn: "academic_program_id");
                    table.ForeignKey(
                        name: "fk_announcements_classrooms_classroom_id",
                        column: x => x.classroom_id,
                        principalTable: "classrooms",
                        principalColumn: "classroom_id");
                    table.ForeignKey(
                        name: "fk_announcements_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "fk_announcements_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    enrollment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    practical_grade = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    exam_grade = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    final_grade = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    classroom_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enrollments", x => x.enrollment_id);
                    table.ForeignKey(
                        name: "fk_enrollments_classrooms_classroom_id",
                        column: x => x.classroom_id,
                        principalTable: "classrooms",
                        principalColumn: "classroom_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_enrollments_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lectures",
                columns: table => new
                {
                    lecture_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    lecture_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    classroom_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lectures", x => x.lecture_id);
                    table.ForeignKey(
                        name: "fk_lectures_classrooms_classroom_id",
                        column: x => x.classroom_id,
                        principalTable: "classrooms",
                        principalColumn: "classroom_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question_options",
                columns: table => new
                {
                    question_option_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "text", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_options", x => x.question_option_id);
                    table.ForeignKey(
                        name: "fk_question_options_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lecture_quizzes",
                columns: table => new
                {
                    lecture_quiz_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lecture_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lecture_quizzes", x => x.lecture_quiz_id);
                    table.ForeignKey(
                        name: "fk_lecture_quizzes_lectures_lecture_id",
                        column: x => x.lecture_id,
                        principalTable: "lectures",
                        principalColumn: "lecture_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "materials",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    material_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    original_filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    file_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lecture_id = table.Column<int>(type: "integer", nullable: true),
                    course_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_materials", x => x.material_id);
                    table.ForeignKey(
                        name: "fk_materials_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "fk_materials_lectures_lecture_id",
                        column: x => x.lecture_id,
                        principalTable: "lectures",
                        principalColumn: "lecture_id");
                });

            migrationBuilder.CreateTable(
                name: "lecture_quiz_questions",
                columns: table => new
                {
                    lecture_quiz_question_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    image_public_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    question_type = table.Column<string>(type: "text", nullable: false),
                    lecture_quiz_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lecture_quiz_questions", x => x.lecture_quiz_question_id);
                    table.ForeignKey(
                        name: "fk_lecture_quiz_questions_lecture_quizzes_lecture_quiz_id",
                        column: x => x.lecture_quiz_id,
                        principalTable: "lecture_quizzes",
                        principalColumn: "lecture_quiz_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lecture_quiz_submissions",
                columns: table => new
                {
                    lecture_quiz_submission_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    score = table.Column<int>(type: "integer", nullable: false),
                    total_questions = table.Column<int>(type: "integer", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    lecture_quiz_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lecture_quiz_submissions", x => x.lecture_quiz_submission_id);
                    table.ForeignKey(
                        name: "fk_lecture_quiz_submissions_lecture_quizzes_lecture_quiz_id",
                        column: x => x.lecture_quiz_id,
                        principalTable: "lecture_quizzes",
                        principalColumn: "lecture_quiz_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lecture_quiz_submissions_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lecture_quiz_question_options",
                columns: table => new
                {
                    lecture_quiz_question_option_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "text", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    lecture_quiz_question_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lecture_quiz_question_options", x => x.lecture_quiz_question_option_id);
                    table.ForeignKey(
                        name: "fk_lecture_quiz_question_options_lecture_quiz_questions_lectur",
                        column: x => x.lecture_quiz_question_id,
                        principalTable: "lecture_quiz_questions",
                        principalColumn: "lecture_quiz_question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_answers",
                columns: table => new
                {
                    student_answer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lecture_quiz_submission_id = table.Column<int>(type: "integer", nullable: false),
                    lecture_quiz_question_id = table.Column<int>(type: "integer", nullable: false),
                    selected_option_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_answers", x => x.student_answer_id);
                    table.ForeignKey(
                        name: "fk_student_answers_lecture_quiz_question_options_selected_opti",
                        column: x => x.selected_option_id,
                        principalTable: "lecture_quiz_question_options",
                        principalColumn: "lecture_quiz_question_option_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_student_answers_lecture_quiz_questions_lecture_quiz_questio",
                        column: x => x.lecture_quiz_question_id,
                        principalTable: "lecture_quiz_questions",
                        principalColumn: "lecture_quiz_question_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_student_answers_lecture_quiz_submissions_lecture_quiz_submi",
                        column: x => x.lecture_quiz_submission_id,
                        principalTable: "lecture_quiz_submissions",
                        principalColumn: "lecture_quiz_submission_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_announcements_academic_program_id",
                table: "announcements",
                column: "academic_program_id");

            migrationBuilder.CreateIndex(
                name: "ix_announcements_classroom_id",
                table: "announcements",
                column: "classroom_id");

            migrationBuilder.CreateIndex(
                name: "ix_announcements_course_id",
                table: "announcements",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "ix_announcements_created_by_user_id",
                table: "announcements",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_archived_enrollments_archived_classroom_id",
                table: "archived_enrollments",
                column: "archived_classroom_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_conversations_user_id",
                table: "chat_conversations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_conversation_id",
                table: "chat_messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_classrooms_course_id",
                table: "classrooms",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "ix_classrooms_teacher_id",
                table: "classrooms",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ix_courses_academic_program_id",
                table: "courses",
                column: "academic_program_id");

            migrationBuilder.CreateIndex(
                name: "ix_courses_coordinator_id",
                table: "courses",
                column: "coordinator_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_classroom_id",
                table: "enrollments",
                column: "classroom_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_student_id",
                table: "enrollments",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_failed_students_academic_program_id",
                table: "failed_students",
                column: "academic_program_id");

            migrationBuilder.CreateIndex(
                name: "ix_graduations_academic_program_id",
                table: "graduations",
                column: "academic_program_id");

            migrationBuilder.CreateIndex(
                name: "ix_lecture_quiz_question_options_lecture_quiz_question_id",
                table: "lecture_quiz_question_options",
                column: "lecture_quiz_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_lecture_quiz_questions_lecture_quiz_id",
                table: "lecture_quiz_questions",
                column: "lecture_quiz_id");

            migrationBuilder.CreateIndex(
                name: "ix_lecture_quiz_submissions_lecture_quiz_id",
                table: "lecture_quiz_submissions",
                column: "lecture_quiz_id");

            migrationBuilder.CreateIndex(
                name: "ix_lecture_quiz_submissions_student_id_lecture_quiz_id",
                table: "lecture_quiz_submissions",
                columns: new[] { "student_id", "lecture_quiz_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lecture_quizzes_lecture_id",
                table: "lecture_quizzes",
                column: "lecture_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lectures_classroom_id",
                table: "lectures",
                column: "classroom_id");

            migrationBuilder.CreateIndex(
                name: "ix_materials_course_id",
                table: "materials",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "ix_materials_lecture_id",
                table: "materials",
                column: "lecture_id");

            migrationBuilder.CreateIndex(
                name: "ix_program_registrations_academic_program_id",
                table: "program_registrations",
                column: "academic_program_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_options_question_id",
                table: "question_options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_questions_course_id",
                table: "questions",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "ix_questions_created_by_id",
                table: "questions",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_questions_reviewed_by_id",
                table: "questions",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_answers_lecture_quiz_question_id",
                table: "student_answers",
                column: "lecture_quiz_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_answers_lecture_quiz_submission_id",
                table: "student_answers",
                column: "lecture_quiz_submission_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_answers_selected_option_id",
                table: "student_answers",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_academic_program_id",
                table: "users",
                column: "academic_program_id");

            migrationBuilder.CreateIndex(
                name: "ix_weekly_challenge_submissions_course_id",
                table: "weekly_challenge_submissions",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "ix_weekly_challenge_submissions_student_id_course_id_year_week",
                table: "weekly_challenge_submissions",
                columns: new[] { "student_id", "course_id", "year", "week_of_year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "announcements");

            migrationBuilder.DropTable(
                name: "archived_enrollments");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "failed_students");

            migrationBuilder.DropTable(
                name: "graduation_certificates");

            migrationBuilder.DropTable(
                name: "materials");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "payment_settings");

            migrationBuilder.DropTable(
                name: "program_registrations");

            migrationBuilder.DropTable(
                name: "question_options");

            migrationBuilder.DropTable(
                name: "student_answers");

            migrationBuilder.DropTable(
                name: "weekly_challenge_submissions");

            migrationBuilder.DropTable(
                name: "archived_classrooms");

            migrationBuilder.DropTable(
                name: "chat_conversations");

            migrationBuilder.DropTable(
                name: "graduations");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "lecture_quiz_question_options");

            migrationBuilder.DropTable(
                name: "lecture_quiz_submissions");

            migrationBuilder.DropTable(
                name: "lecture_quiz_questions");

            migrationBuilder.DropTable(
                name: "lecture_quizzes");

            migrationBuilder.DropTable(
                name: "lectures");

            migrationBuilder.DropTable(
                name: "classrooms");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "academic_programs");
        }
    }
}
