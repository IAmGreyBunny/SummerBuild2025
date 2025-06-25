<?php
header('Content-Type: application/json');
require 'db_connect.php';

// Default response
$response = [
    "status_code" => 0,
    "error_message" => ""
];

// Get the raw POST data
$json = json_decode(file_get_contents('php://input'), true);

// --- Input Validation ---
if ($json === null) {
    $response["status_code"] = 7;
    $response["error_message"] = "Invalid JSON provided.";
    echo json_encode($response);
    exit();
}

// Note: In your SQL example, you used 'owner_id', but your other scripts use 'player_id'.
// This script will use 'player_id' for consistency.
$player_id = $json['player_id'] ?? null;
$item_id = $json['item_id'] ?? null;
$quantity = $json['quantity'] ?? null;

if (!$player_id) {
    $response["status_code"] = 10;
    $response["error_message"] = "player_id not provided.";
    echo json_encode($response);
    exit();
}
if (!$item_id) {
    $response["status_code"] = 31; // Updated error code
    $response["error_message"] = "item_id not provided.";
    echo json_encode($response);
    exit();
}
// We check for null specifically because a quantity of 0 is a valid value.
if ($quantity === null) {
    $response["status_code"] = 32; // Updated error code
    $response["error_message"] = "quantity value not provided.";
    echo json_encode($response);
    exit();
}

// --- Database Query using "INSERT ... ON DUPLICATE KEY UPDATE" ---
// This single statement will:
// 1. INSERT a new row if the (player_id, item_id) combination does not exist.
// 2. UPDATE the existing row's quantity if the combination already exists.
$upsertQuery = "INSERT INTO inventory (player_id, item_id, quantity) VALUES (?, ?, ?)
                ON DUPLICATE KEY UPDATE quantity = VALUES(quantity)";
                
// Note: We use `quantity = VALUES(quantity)` instead of `quantity = ?`.
// This tells MySQL to use the 'quantity' value from the INSERT part of the statement for the update.
// This is ideal for your C# script which sends the new total, not an increment.

$stmt = mysqli_prepare($con, $upsertQuery);

if ($stmt) {
    // Bind the parameters: player_id, item_id, and the new quantity.
    mysqli_stmt_bind_param($stmt, "iii", $player_id, $item_id, $quantity);
    
    if (mysqli_stmt_execute($stmt)) {
        // Success! The response is already set to status_code 0.
        // We can optionally add info about whether it was an insert or update.
        if (mysqli_stmt_affected_rows($stmt) == 1) {
            $response["action"] = "inserted";
        } elseif (mysqli_stmt_affected_rows($stmt) == 2) {
            $response["action"] = "updated";
        }
    } else {
        $response["status_code"] = 33; // Updated error code
        $response["error_message"] = "Upsert Inventory Query Failed: " . mysqli_stmt_error($stmt);
    }
    mysqli_stmt_close($stmt);
} else {
    $response["status_code"] = 34; // Updated error code
    $response["error_message"] = "Failed to prepare the database statement.";
}

mysqli_close($con);
echo json_encode($response);
?>
