<?php
header('Content-Type: application/json');

// Error info
$status_code = 0;
$error_message = "";

//Connects to sql database
require 'db_connect.php';


//Query Shop
$getShopItemsQuery = "SELECT * FROM items";
$getShopItemsQueryResult = mysqli_query($con,$getShopItemsQuery);

if (!$getShopItemsQueryResult) {
	echo json_encode([
		"status_code" => 9,
		"error_message" => "Get Shop Item Query Failed"
	]);
	exit();
}
else
{

	// Collect data
	$items = [];
	while ($row = mysqli_fetch_assoc($getShopItemsQueryResult)) {
		$items[] = $row;
	}
	

	echo json_encode([
		"status_code" => $status_code,
		"error_message" => $error_message,
    	"items" => $items
	]);
}

?>