import json
import os
import uuid
from pathlib import Path

POSTMAN_DIR = Path(r"D:\Abdallah\Projects\Gymora\gymora_Backend\Postman")
OUTPUT_FILE = POSTMAN_DIR / "Gymora Complete Collection.postman_collection.json"

COLLECTION_ORDER = [
    "00 - Test.json",
    "01 - Authentication.json",
    "02 - SubscriptionPlan.json",
    "03 - PaymentRequest.json",
    "04 - Coupon.json",
    "05 - CouponRedemption.json",
    "06 - OwnerSubscription.json",
    "07 - Gym.json",
    "08 - GymPerson.json",
    "09 - Invitation.json",
    "09 - Users.json",
    "10 - Membership Plans.json",
    "11 - Attendance.json",
    "12 - Coach Assignment.json",
    "13 - Body Measurements.json",
    "14 - Revenues.json",
    "15 - Expenses.json",
    "16 - Reports.json",
    "17 - Exercises.json",
    "18 - Workout Plans.json",
    "19 - Member Workout Plans.json",
    "20 - User Workout Blocks.json",
    "21 - Sessions.json",
    "22 - Session Exercises.json",
]

CRUD_FOLDER_NAME = "01 - CRUD Verification"
SECURITY_FOLDER_NAME = "02 - Security & Isolation Boundaries"
CACHING_FOLDER_NAME = "03 - Caching & Auditing"
USER_STORIES_FOLDER_NAME = "04 - Creative User Stories"

ENTITY_MAP = {
    "Test": "Test",
    "Auth": "Auth",
    "SubscriptionPlan": "SubscriptionPlan",
    "PaymentRequest": "PaymentRequest",
    "Coupon": "Coupon",
    "CouponRedemption": "CouponRedemption",
    "OwnerSubscription": "OwnerSubscription",
    "Gym": "Gym",
    "GymPerson": "GymPerson",
    "Invitation": "Invitation",
    "Users": "User",
    "Membership Plans": "MembershipPlan",
    "Attendance": "Attendance",
    "Coach Assignment": "CoachAssignment",
    "Body Measurements": "BodyMeasurement",
    "Revenues": "Revenue",
    "Expenses": "Expense",
    "Reports": "Report",
    "Exercises": "Exercise",
    "Workout Plans": "WorkoutPlan",
    "Member Workout Plans": "MemberWorkoutPlan",
    "User Workout Blocks": "UserWorkoutBlock",
    "Sessions": "Session",
    "Session Exercises": "SessionExercise",
}

FILTER_MAP = {
    "Gym": {"search_term": "Fitness", "order_by": "Name", "between_filters": '"Latitude": {"min":"29.9","max":"31.0"},"Longitude": {"min":"31.0","max":"32.0"}', "exact_filters": '"Status": ["1"]'},
    "GymPerson": {"search_term": "Member", "order_by": "Name", "between_filters": "", "exact_filters": '"PersonType": ["0"],"AccessStatus": ["1"]'},
    "Membership Plans": {"search_term": "Premium", "order_by": "Name", "between_filters": '"Price": {"min":"10.0","max":"100.0"}', "exact_filters": '"DurationDays": ["30"]'},
    "Revenues": {"search_term": "", "order_by": "RevenueDate", "between_filters": '"RevenueDate": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"}', "exact_filters": '"RevenueCategory": ["0"]'},
    "Expenses": {"search_term": "", "order_by": "ExpenseDate", "between_filters": '"ExpenseDate": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"}', "exact_filters": '"ExpenseCategory": ["0"]'},
    "Exercises": {"search_term": "Push", "order_by": "Name", "between_filters": "", "exact_filters": '"PrimaryMuscle": ["0"],"DifficultyLevel": ["0"]'},
    "Workout Plans": {"search_term": "Strength", "order_by": "Id", "between_filters": "", "exact_filters": ""},
    "Sessions": {"search_term": "Day", "order_by": "DayNumber", "between_filters": "", "exact_filters": ""},
    "Invitation": {"search_term": "", "order_by": "Id", "between_filters": "", "exact_filters": '"Status": ["0"],"GymRole": ["10"],"UserId": ["5"]'},
    "Coupon": {"search_term": "SUMMER", "order_by": "Name", "between_filters": '"DiscountValue": {"min":"5.0","max":"50.0"},"ValidFrom": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"}', "exact_filters": '"DiscountType": ["1"]'},
    "SubscriptionPlan": {"search_term": "Pro", "order_by": "Name", "between_filters": '"MaxOwnedGyms": {"min":"1","max":"10"}', "exact_filters": ""},
    "PaymentRequest": {"search_term": "", "order_by": "Id", "between_filters": '"OriginalAmount": {"min":"10.0","max":"500.0"}', "exact_filters": '"Status": ["0"]'},
    "Coach Assignment": {"search_term": "", "order_by": "AssignedAt", "between_filters": '"AssignedAt": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"},"EndedAt": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"}', "exact_filters": '"MemberId": ["1"],"CoachStaffId": ["2"]'},
    "Body Measurements": {"search_term": "", "order_by": "Id", "between_filters": "", "exact_filters": ""},
    "Attendance": {"search_term": "Staff", "order_by": "CheckInTime", "between_filters": '"CheckInTime": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"}', "exact_filters": '"EntryMethod": ["Staff Override"]'},
    "Reports": {"search_term": "", "order_by": "Id", "between_filters": "", "exact_filters": ""},
    "Member Workout Plans": {"search_term": "", "order_by": "Id", "between_filters": '"StartDate": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"}', "exact_filters": '"Status": ["0"]'},
    "User Workout Blocks": {"search_term": "", "order_by": "Id", "between_filters": "", "exact_filters": ""},
    "Session Exercises": {"search_term": "", "order_by": "OrderIndex", "between_filters": "", "exact_filters": ""},
    "OwnerSubscription": {"search_term": "", "order_by": "Id", "between_filters": '"StartDate": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"}', "exact_filters": '"CurrencyCode": ["USD"]'},
    "CouponRedemption": {"search_term": "", "order_by": "Id", "between_filters": '"DiscountAmount": {"min":"1.0","max":"100.0"}', "exact_filters": '"CouponId": ["1"]'},
    "Users": {"search_term": "Admin", "order_by": "Id", "between_filters": '"CreatedOn": {"min":"2026-01-01T00:00:00Z","max":"2026-12-31T23:59:59Z"}', "exact_filters": '"PersonName": ["Admin"]'},
    "Test": {"search_term": "", "order_by": "Id", "between_filters": "", "exact_filters": ""},
}


def load_collection(filename):
    filepath = POSTMAN_DIR / filename
    if not filepath.exists():
        print(f"WARNING: {filename} not found, skipping")
        return None
    with open(filepath, "r", encoding="utf-8-sig") as f:
        return json.load(f)


def get_auth_for_folder(folder_name):
    if folder_name in ["00 - Test"]:
        return {"type": "noauth"}
    if folder_name in ["01 - Authentication"]:
        return None
    return {
        "type": "bearer",
        "bearer": [{"key": "token", "value": "{{user1_token}}", "type": "string"}]
    }


def make_test_script(lines):
    return {
        "listen": "test",
        "script": {"exec": lines, "type": "text/javascript"}
    }


def enrich_request_with_chaining(item, folder_name, entity_name, request_name, next_request=None):
    req = item.get("request", {})
    method = req.get("method", "GET").upper()
    url_raw = req.get("url", {}).get("raw", "") or ""
    body_raw = ""
    if req.get("body") and req["body"].get("mode") == "raw":
        body_raw = req["body"].get("raw", "")

    is_paged = "filters" in body_raw
    is_create = "/Create" in url_raw or request_name.lower().startswith("create")
    is_get_by_id = "{{last" in url_raw
    is_delete = method == "DELETE"

    next_cmd = f'postman.setNextRequest("{next_request}");' if next_request else 'postman.setNextRequest(null);'

    if is_paged:
        lines = [
            'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
            'pm.test("Response is valid JSON", function () { pm.response.to.be.json; });',
            'pm.test("IsSuccess is true", function () { var d = pm.response.json(); pm.expect(d.isSuccess).to.eql(true); });',
            'pm.test("Pagination has items", function () { var d = pm.response.json(); pm.expect(d.data).to.have.property("items"); });',
        ]
        if entity_name not in ("Test", "Auth"):
            var_name = f"last{entity_name}Id"
            lines.append(f'var d = pm.response.json();')
            lines.append(f'if (d && d.data && d.data.items && d.data.items.length > 0) {{ pm.collectionVariables.set("{var_name}", d.data.items[0].id); }}')
        lines.append(next_cmd)
        item["event"] = [make_test_script(lines)]

    elif is_create:
        lines = [
            'pm.test("Status code is 201", function () { pm.response.to.have.status(201); });',
            'pm.test("Response is valid JSON", function () { pm.response.to.be.json; });',
            'pm.test("IsSuccess is true", function () { var d = pm.response.json(); pm.expect(d.isSuccess).to.eql(true); });',
        ]
        if entity_name not in ("Test", "Auth"):
            var_name = f"last{entity_name}Id"
            lines.append(f'var d = pm.response.json();')
            lines.append(f'if (d && d.data && d.data.id) {{ pm.collectionVariables.set("{var_name}", d.data.id); }}')

        if folder_name == "Auth" and "Login" in request_name:
            if "SuperAdmin" in request_name:
                lines.extend([
                    'var d = pm.response.json();',
                    'if (d.data && d.data.accessToken) {',
                    '    pm.environment.set("superAdmin_token", d.data.accessToken);',
                    '    pm.environment.set("superAdmin_refreshToken", d.data.refreshToken);',
                    '    if (d.data.currentGym && d.data.currentGym.gymId) {',
                    '        pm.environment.set("superAdmin_gymId", d.data.currentGym.gymId);',
                    '    }',
                    '}',
                ])
            elif "First User" in request_name:
                lines.extend([
                    'var d = pm.response.json();',
                    'if (d.data && d.data.accessToken) {',
                    '    pm.environment.set("user1_token", d.data.accessToken);',
                    '    pm.environment.set("user1_refreshToken", d.data.refreshToken);',
                    '    if (d.data.currentGym && d.data.currentGym.gymId) {',
                    '        pm.environment.set("user1_gymId", d.data.currentGym.gymId);',
                    '        pm.environment.set("gymId", d.data.currentGym.gymId);',
                    '    }',
                    '}',
                ])
            elif "Second User" in request_name:
                lines.extend([
                    'var d = pm.response.json();',
                    'if (d.data && d.data.accessToken) {',
                    '    pm.environment.set("user2_token", d.data.accessToken);',
                    '    pm.environment.set("user2_refreshToken", d.data.refreshToken);',
                    '    if (d.data.currentGym && d.data.currentGym.gymId) {',
                    '        pm.environment.set("user2_gymId", d.data.currentGym.gymId);',
                    '    }',
                    '}',
                ])

        lines.append(next_cmd)
        item["event"] = [make_test_script(lines)]

    elif is_get_by_id:
        lines = [
            'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
            'pm.test("Response is valid JSON", function () { pm.response.to.be.json; });',
            'pm.test("IsSuccess is true", function () { var d = pm.response.json(); pm.expect(d.isSuccess).to.eql(true); });',
            next_cmd,
        ]
        item["event"] = [make_test_script(lines)]

    elif is_delete:
        lines = [
            'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
            'pm.test("IsSuccess is true", function () { var d = pm.response.json(); pm.expect(d.isSuccess).to.eql(true); });',
            next_cmd,
        ]
        item["event"] = [make_test_script(lines)]

    else:
        lines = [
            'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
            'pm.test("Response is valid JSON", function () { pm.response.to.be.json; });',
            'pm.test("IsSuccess is true", function () { var d = pm.response.json(); pm.expect(d.isSuccess).to.eql(true); });',
            next_cmd,
        ]
        item["event"] = [make_test_script(lines)]

    return item


def enrich_pagination_body(item, folder_name):
    body = item.get("request", {}).get("body", {})
    if not body or body.get("mode") != "raw":
        return item
    raw = body.get("raw", "")
    if "filters" not in raw:
        return item

    fc = FILTER_MAP.get(folder_name, {"search_term": "", "order_by": "Id", "between_filters": "", "exact_filters": ""})
    new_body = json.dumps({
        "pageNumber": 1,
        "pageSize": 10,
        "searchTerm": fc["search_term"],
        "orderBy": fc["order_by"],
        "orderDirection": "asc",
        "filters": {
            "betweenFilters": json.loads("{" + fc["between_filters"] + "}") if fc["between_filters"] else {},
            "exactFilters": json.loads("{" + fc["exact_filters"] + "}") if fc["exact_filters"] else {},
        }
    }, indent=2)
    item["request"]["body"]["raw"] = new_body
    return item


def create_chaining_folder(items, folder_name, entity_name):
    requests = [i for i in items if "request" in i]
    if len(requests) < 2:
        return items
    for i in range(len(requests) - 1):
        enrich_request_with_chaining(requests[i], folder_name, entity_name, requests[i].get("name", ""), next_request=requests[i+1].get("name", ""))
    enrich_request_with_chaining(requests[-1], folder_name, entity_name, requests[-1].get("name", ""), next_request=None)
    return items


def create_security_test_folder():
    items = []
    gym_entities = {
        "GymPerson": "/api/GymPerson",
        "MembershipPlan": "/api/MembershipPlans",
        "Revenue": "/api/Revenues",
        "Expense": "/api/Expenses",
        "Exercise": "/api/Exercises",
        "WorkoutPlan": "/api/WorkoutPlans",
        "Session": "/api/Sessions",
        "Invitation": "/api/Invitation",
        "CoachAssignment": "/api/CoachAssignment/get-gym-coach-assignments",
        "BodyMeasurement": "/api/BodyMeasurements",
        "Attendance": "/api/Attendance/history",
    }

    for entity_name, route in gym_entities.items():
        items.append({
            "name": f"Cross-Gym Access: {entity_name} (User2)",
            "request": {
                "method": "POST",
                "header": [{"key": "Content-Type", "value": "application/json"}],
                "body": {"mode": "raw", "raw": json.dumps({"pageNumber": 1, "pageSize": 10, "orderBy": "Id", "orderDirection": "asc"})},
                "url": {"raw": "{{baseUrl}}" + route, "host": ["{{baseUrl}}"], "path": route.strip("/").split("/")}
            },
            "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{user2_token}}", "type": "string"}]},
            "event": [make_test_script([
                'if (pm.response.code === 404) {',
                '    pm.test("Cross-gym returns 404", function () { pm.response.to.have.status(404); });',
                '} else if (pm.response.code === 403) {',
                '    pm.test("Cross-gym returns 403", function () { pm.response.to.have.status(403); });',
                '} else {',
                '    pm.test("Expected 404 or 403, got " + pm.response.code, function () { pm.expect(pm.response.code).to.be.oneOf([403, 404]); });',
                '}',
            ])],
            "response": []
        })

        items.append({
            "name": f"SuperAdmin Override: {entity_name}",
            "request": {
                "method": "POST",
                "header": [{"key": "Content-Type", "value": "application/json"}],
                "body": {"mode": "raw", "raw": json.dumps({"pageNumber": 1, "pageSize": 10, "orderBy": "Id", "orderDirection": "asc"})},
                "url": {"raw": "{{baseUrl}}" + route, "host": ["{{baseUrl}}"], "path": route.strip("/").split("/")}
            },
            "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{superAdmin_token}}", "type": "string"}]},
            "event": [make_test_script([
                'pm.test("SuperAdmin can access any gym resource", function () { pm.response.to.have.status(200); });',
            ])],
            "response": []
        })

    return items


def create_user_stories_folder():
    items = []

    items.append({
        "name": "Story: Member Onboarding",
        "description": "Create GymPerson (Member) -> Get Member -> Create Invitation",
        "item": [
            {
                "name": "1. Create Member (GymPerson)",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"personType": 1, "name": "New Member", "phoneNumber": "+1234567899", "email": "newmember@test.com", "gender": "Male", "staffProfile": None, "memberProfile": {"medicalNotes": "No allergies", "notes": "VIP"}})},
                    "url": {"raw": "{{baseUrl}}/api/GymPerson/Create", "host": ["{{baseUrl}}"], "path": ["api", "GymPerson", "Create"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastGymPersonId", d.data.id); }',
                    'postman.setNextRequest("2. Get Member By Id");',
                ])],
                "response": []
            },
            {
                "name": "2. Get Member By Id",
                "request": {"method": "GET", "url": {"raw": "{{baseUrl}}/api/GymPerson/{{lastGymPersonId}}", "host": ["{{baseUrl}}"], "path": ["api", "GymPerson", "{{lastGymPersonId}}"]}},
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'postman.setNextRequest("3. Create Invitation for Member");',
                ])],
                "response": []
            },
            {
                "name": "3. Create Invitation for Member",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"userId": 2, "gymRole": 6, "membership": {"membershipPlanId": 1, "discountAmount": 0}, "salary": None})},
                    "url": {"raw": "{{baseUrl}}/api/Invitation/Create", "host": ["{{baseUrl}}"], "path": ["api", "Invitation", "Create"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastInvitationId", d.data.id); }',
                    'postman.setNextRequest(null);',
                ])],
                "response": []
            },
        ],
        "response": []
    })

    items.append({
        "name": "Story: Subscription Purchase",
        "description": "List Plans -> Get Plan -> Create Payment Request",
        "item": [
            {
                "name": "1. List Subscription Plans",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"pageNumber": 1, "pageSize": 10, "orderBy": "Id", "orderDirection": "asc"})},
                    "url": {"raw": "{{baseUrl}}/api/SubscriptionPlan", "host": ["{{baseUrl}}"], "path": ["api", "SubscriptionPlan"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.items && d.data.items.length > 0) { pm.collectionVariables.set("lastSubscriptionPlanId", d.data.items[0].id); }',
                    'postman.setNextRequest("2. Get Plan By Id");',
                ])],
                "response": []
            },
            {
                "name": "2. Get Plan By Id",
                "request": {"method": "GET", "url": {"raw": "{{baseUrl}}/api/SubscriptionPlan/{{lastSubscriptionPlanId}}", "host": ["{{baseUrl}}"], "path": ["api", "SubscriptionPlan", "{{lastSubscriptionPlanId}}"]}},
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'postman.setNextRequest("3. Create Payment Request");',
                ])],
                "response": []
            },
            {
                "name": "3. Create Payment Request",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "multipart/form-data"}],
                    "body": {"mode": "formdata", "formdata": [
                        {"key": "PlanId", "value": "{{lastSubscriptionPlanId}}", "type": "text"},
                        {"key": "PlanPriceId", "value": "1", "type": "text"},
                        {"key": "CouponCode", "value": "", "type": "text"},
                        {"key": "File", "type": "file", "src": ""}
                    ]},
                    "url": {"raw": "{{baseUrl}}/api/PaymentRequest/Create", "host": ["{{baseUrl}}"], "path": ["api", "PaymentRequest", "Create"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastPaymentRequestId", d.data.id); }',
                    'postman.setNextRequest(null);',
                ])],
                "response": []
            },
        ],
        "response": []
    })

    items.append({
        "name": "Story: Content Publishing (Exercise -> WorkoutPlan)",
        "description": "Creates an Exercise, then a WorkoutPlan with Sessions.",
        "item": [
            {
                "name": "1. Create Exercise",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"name": "Flow Bench Press", "description": "Barbell bench press", "primaryMuscle": 0, "secondaryMuscle": 3, "equipment": 0, "difficultyLevel": 1, "videoUrl": ""})},
                    "url": {"raw": "{{baseUrl}}/api/Exercises/Create", "host": ["{{baseUrl}}"], "path": ["api", "Exercises", "Create"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 201", function () { pm.response.to.have.status(201); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastExerciseId", d.data.id); }',
                    'postman.setNextRequest("2. Create WorkoutPlan");',
                ])],
                "response": []
            },
            {
                "name": "2. Create WorkoutPlan",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"planName": "Flow Strength Plan", "description": "4-week strength", "sessions": [{"workoutPlanId": 0, "dayNumber": 1, "sessionName": "Day 1 - Chest", "exercises": [{"sessionId": 0, "exerciseId": 0, "exerciseName": "Bench Press", "sets": 4, "reps": 8, "weightKg": 80.0, "restSeconds": 90, "notes": "Warm up", "orderIndex": 0}]}]})},
                    "url": {"raw": "{{baseUrl}}/api/WorkoutPlans/Create", "host": ["{{baseUrl}}"], "path": ["api", "WorkoutPlans", "Create"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 201", function () { pm.response.to.have.status(201); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastWorkoutPlanId", d.data.id); }',
                    'postman.setNextRequest(null);',
                ])],
                "response": []
            },
        ],
        "response": []
    })

    items.append({
        "name": "Story: Revenue & Expense Tracking",
        "description": "Creates Revenue and Expense entries, then gets Financial Reports.",
        "item": [
            {
                "name": "1. Create Revenue",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"revenueCategory": 0, "gymMemberId": 1, "amount": 150.00, "paymentMethod": 0, "referenceNumber": "REV-001", "description": "Monthly fee", "revenueDate": "2026-08-01T00:00:00Z"})},
                    "url": {"raw": "{{baseUrl}}/api/Revenues/Create", "host": ["{{baseUrl}}"], "path": ["api", "Revenues", "Create"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 201", function () { pm.response.to.have.status(201); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastRevenueId", d.data.id); }',
                    'postman.setNextRequest("2. Create Expense");',
                ])],
                "response": []
            },
            {
                "name": "2. Create Expense",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"expenseCategory": 0, "gymStaffId": 1, "amount": 50.00, "paymentMethod": 0, "referenceNumber": "EXP-001", "receiptUrl": "", "description": "Supplies", "expenseDate": "2026-08-01T00:00:00Z"})},
                    "url": {"raw": "{{baseUrl}}/api/Expenses/Create", "host": ["{{baseUrl}}"], "path": ["api", "Expenses", "Create"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 201", function () { pm.response.to.have.status(201); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastExpenseId", d.data.id); }',
                    'postman.setNextRequest("3. Get Revenue Report");',
                ])],
                "response": []
            },
            {
                "name": "3. Get Revenue Report",
                "request": {
                    "method": "GET",
                    "url": {"raw": "{{baseUrl}}/api/v1/gyms/{{gymId}}/finances/reports/revenue?fromDate=2026-01-01&toDate=2026-12-31", "host": ["{{baseUrl}}"], "path": ["api", "v1", "gyms", "{{gymId}}", "finances", "reports", "revenue"], "query": [{"key": "fromDate", "value": "2026-01-01"}, {"key": "toDate", "value": "2026-12-31"}]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'postman.setNextRequest("4. Get Expense Report");',
                ])],
                "response": []
            },
            {
                "name": "4. Get Expense Report",
                "request": {
                    "method": "GET",
                    "url": {"raw": "{{baseUrl}}/api/v1/gyms/{{gymId}}/finances/reports/expense?fromDate=2026-01-01&toDate=2026-12-31", "host": ["{{baseUrl}}"], "path": ["api", "v1", "gyms", "{{gymId}}", "finances", "reports", "expense"], "query": [{"key": "fromDate", "value": "2026-01-01"}, {"key": "toDate", "value": "2026-12-31"}]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'postman.setNextRequest(null);',
                ])],
                "response": []
            },
        ],
        "response": []
    })

    items.append({
        "name": "Story: Staff Management (Coach -> Assign -> Pay)",
        "description": "Creates a Coach, assigns a Member, then pays salary.",
        "item": [
            {
                "name": "1. Create Coach",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"personType": 0, "name": "Flow Coach", "phoneNumber": "+1234567893", "email": "flowcoach@test.com", "gender": "Male", "staffProfile": {"gymRoleId": 2, "salary": 3000.00, "salaryValidFrom": "2026-08-01", "salaryValidUntil": "2026-12-31"}, "memberProfile": None})},
                    "url": {"raw": "{{baseUrl}}/api/GymPerson/Create", "host": ["{{baseUrl}}"], "path": ["api", "GymPerson", "Create"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastCoachStaffId", d.data.id); }',
                    'postman.setNextRequest("2. Assign Member to Coach");',
                ])],
                "response": []
            },
            {
                "name": "2. Assign Member to Coach",
                "request": {
                    "method": "POST",
                    "header": [{"key": "Content-Type", "value": "application/json"}],
                    "body": {"mode": "raw", "raw": json.dumps({"memberId": 1, "coachStaffId": 0})},
                    "url": {"raw": "{{baseUrl}}/api/CoachAssignment/coach-assignments", "host": ["{{baseUrl}}"], "path": ["api", "CoachAssignment", "coach-assignments"]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 201", function () { pm.response.to.have.status(201); });',
                    'var d = pm.response.json();',
                    'if (d.data && d.data.id) { pm.collectionVariables.set("lastCoachAssignmentId", d.data.id); }',
                    'postman.setNextRequest("3. Pay Coach Salary");',
                ])],
                "response": []
            },
            {
                "name": "3. Pay Coach Salary",
                "request": {
                    "method": "POST",
                    "url": {"raw": "{{baseUrl}}/api/GymPerson/{{lastCoachStaffId}}/pay-salary?salaryValidFrom=2026-08-01&salaryValidUntil=2026-08-31", "host": ["{{baseUrl}}"], "path": ["api", "GymPerson", "{{lastCoachStaffId}}", "pay-salary"], "query": [{"key": "salaryValidFrom", "value": "2026-08-01"}, {"key": "salaryValidUntil", "value": "2026-08-31"}]}
                },
                "event": [make_test_script([
                    'pm.test("Status code is 200", function () { pm.response.to.have.status(200); });',
                    'postman.setNextRequest(null);',
                ])],
                "response": []
            },
        ],
        "response": []
    })

    return items


def build_merged_collection():
    all_api_folders = []

    for filename in COLLECTION_ORDER:
        collection = load_collection(filename)
        if collection is None:
            continue
        folder_name = collection.get("info", {}).get("name", filename.replace(".json", ""))
        items = collection.get("item", [])
        entity_name = ENTITY_MAP.get(folder_name, folder_name)
        items = create_chaining_folder(items, folder_name, entity_name)
        for item in items:
            if "request" in item:
                item = enrich_pagination_body(item, folder_name)
                auth = get_auth_for_folder(folder_name)
                if auth and "auth" not in item:
                    item["auth"] = auth
        all_api_folders.append({
            "name": folder_name,
            "item": items,
            "auth": get_auth_for_folder(folder_name),
            "response": []
        })

    crud_folder = {"name": CRUD_FOLDER_NAME, "description": "Full CRUD lifecycle for every entity.", "item": [], "response": []}
    for folder in all_api_folders:
        fn = folder["name"]
        en = ENTITY_MAP.get(fn, fn)
        if fn in ("00 - Test", "01 - Authentication"):
            continue
        items = folder.get("item", [])
        create_item = read_item = update_item = delete_item = None
        for it in items:
            if "request" not in it:
                continue
            url = it["request"].get("url", {}).get("raw", "") or ""
            method = it["request"].get("method", "").upper()
            if "/Create" in url:
                create_item = it
            elif method == "GET" and "{{last" in url:
                read_item = it
            elif method == "PUT":
                update_item = it
            elif method == "DELETE":
                delete_item = it
        if create_item and delete_item:
            crud_item = {"name": f"{en} CRUD Lifecycle", "description": f"Create -> Read -> Update -> Delete -> Verify 404 for {en}", "item": [], "response": []}
            for label, src, nxt in [
                ("Create", create_item, f"Read {en}"),
                ("Read", read_item, f"Update {en}"),
                ("Update", update_item, f"Delete {en}"),
                ("Delete", delete_item, None),
            ]:
                if src:
                    ci = json.loads(json.dumps(src))
                    ci["name"] = f"{label} {en}"
                    enrich_request_with_chaining(ci, fn, en, f"{label} {en}", next_request=nxt)
                    crud_item["item"].append(ci)
            verify = {
                "name": f"Verify {en} Deleted (404)",
                "request": {"method": "GET", "url": {"raw": "{{baseUrl}}/api/" + fn.replace(" ", "") + "/{{last" + en + "Id}}", "host": ["{{baseUrl}}"], "path": ["api", fn.replace(" ", ""), "{{last" + en + "Id}}"]}},
                "event": [make_test_script(['pm.test("Deleted entity returns 404", function () { pm.response.to.have.status(404); });'])],
                "response": []
            }
            crud_item["item"].append(verify)
            crud_folder["item"].append(crud_item)

    security_folder = {"name": SECURITY_FOLDER_NAME, "description": "Multi-Tenant and Ownership isolation tests.", "item": create_security_test_folder(), "response": []}

    caching_folder = {"name": CACHING_FOLDER_NAME, "description": "Cache hit/miss/invalidation for ICacheableEntity.", "item": [
        {"name": "MembershipPlan Cache Test", "description": "Tests cache behavior.", "item": [
            {"name": "1. Cache Miss (First Request)", "request": {"method": "POST", "header": [{"key": "Content-Type", "value": "application/json"}], "body": {"mode": "raw", "raw": json.dumps({"pageNumber": 1, "pageSize": 10, "orderBy": "Id", "orderDirection": "asc"})}, "url": {"raw": "{{baseUrl}}/api/MembershipPlans", "host": ["{{baseUrl}}"], "path": ["api", "MembershipPlans"]}}, "event": [make_test_script(['pm.test("Status code is 200", function () { pm.response.to.have.status(200); });', 'postman.setNextRequest("2. Cache Hit");'])], "response": []},
            {"name": "2. Cache Hit", "request": {"method": "POST", "header": [{"key": "Content-Type", "value": "application/json"}], "body": {"mode": "raw", "raw": json.dumps({"pageNumber": 1, "pageSize": 10, "orderBy": "Id", "orderDirection": "asc"})}, "url": {"raw": "{{baseUrl}}/api/MembershipPlans", "host": ["{{baseUrl}}"], "path": ["api", "MembershipPlans"]}}, "event": [make_test_script(['pm.test("Status code is 200", function () { pm.response.to.have.status(200); });', 'postman.setNextRequest(null);'])], "response": []},
        ], "response": []}
    ], "response": []}

    user_stories_folder = {"name": USER_STORIES_FOLDER_NAME, "description": "End-to-end business flow tests.", "item": create_user_stories_folder(), "response": []}

    auth_folder = {"name": "00 - Authentication & Setup", "description": "Login, Switch Gym, Token management. Run first.", "item": [], "response": []}
    auth_api_folder = None
    for folder in all_api_folders:
        if "Auth" in folder["name"]:
            auth_folder["item"] = folder["item"]
            auth_api_folder = folder
            break

    other_api_folders = [f for f in all_api_folders if f is not auth_api_folder]

    merged = {
        "info": {
            "_postman_id": str(uuid.uuid4()),
            "name": "Gymora Complete Collection",
            "description": "Complete Gymora Backend Integration Test Suite.\n\n## Structure\n- **00 - Authentication & Setup**: Login, Switch Gym, Token management\n- **API Endpoints**: All 25 controllers, 133 endpoints\n- **01 - CRUD Verification**: Full lifecycle tests\n- **02 - Security & Isolation Boundaries**: Multi-tenant and Ownership tests\n- **03 - Caching & Auditing**: Cache behavior tests\n- **04 - Creative User Stories**: End-to-end business flows\n\n## Prerequisites\n1. Select **Development** environment\n2. Run **00 - Authentication & Setup** first\n3. Run **Switch Gym** requests\n\n## Variables\n- `{{baseUrl}}`: API base URL\n- `{{user1_token}}`, `{{user2_token}}`, `{{superAdmin_token}}`: Auth tokens\n- `{{gymId}}`: Active gym context\n- `last<EntityName>Id`: Auto-captured entity IDs",
            "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
        },
        "item": [auth_folder] + other_api_folders + [crud_folder, security_folder, caching_folder, user_stories_folder],
        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{user1_token}}", "type": "string"}]},
        "variable": [],
        "event": []
    }

    return merged


def main():
    print("Building merged Postman Collection v2.1...")
    collection = build_merged_collection()
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(collection, f, indent=4, ensure_ascii=False)
    print(f"Written to: {OUTPUT_FILE}")
    total = 0
    def count(items):
        nonlocal total
        for i in items:
            if "request" in i:
                total += 1
            if "item" in i:
                count(i["item"])
    count(collection["item"])
    print(f"Total requests: {total}")
    print(f"Top-level folders: {len(collection['item'])}")
    for item in collection["item"]:
        sc = 0
        def cs(items):
            nonlocal sc
            for i in items:
                if "request" in i:
                    sc += 1
                if "item" in i:
                    cs(i["item"])
        cs(item.get("item", []))
        print(f"  {item['name']}: {sc} requests")


if __name__ == "__main__":
    main()
