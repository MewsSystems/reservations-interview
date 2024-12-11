import { Box, Card, Flex, Heading, Inset } from "@radix-ui/themes";
import { Link, useNavigate } from "@tanstack/react-router";
import { useState } from "react";

function handleLogin(accessCode: string, navigate: ReturnType<typeof useNavigate>) {
  fetch("/api/staff/login", {
    method: "GET",
    headers: {
      "X-Staff-Code": accessCode,
    },
  })
    .then((response) => {
      if (response.ok) {
        alert("Login successful!");
        navigate({ to: "/upcomingReservations" }); 
      } else {
        alert("Invalid access code. Please try again.");
      }
    })
    .catch((error) => {
      console.error("Error during login:", error);
      alert("An error occurred while logging in. Please try again.");
    });
}

export function LandingPage() {
  const navigate = useNavigate();

  const handleButtonClick = () => {
    const accessCode = prompt("Enter your staff access code:");
    if (!accessCode) {
      alert("Access code is required.");
      return;
    }
    handleLogin(accessCode, navigate);
  };

  return (
    <Flex direction="row" align="center" justify="center" gap="9" pt="9">
      <Card size="3" asChild variant="classic">
        <a href="#" onClick={handleButtonClick}>
          <Inset side="top" pb="current">
            <img
              src="https://images.unsplash.com/photo-1550527882-b71dea5f8089?q=80&w=240&h=360&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
              alt="Key on wood board"
              style={{
                width: 240,
                height: 360,
              }}
            />
          </Inset>
          <Heading align="center">Staff Login</Heading>
        </a>
      </Card>
      <Card size="3" asChild variant="classic">
        <Link to="/reservations" preload="intent">
          <Inset clip="padding-box" side="top" pb="current">
            <img
              src="https://images.unsplash.com/photo-1531576788337-610fa9c67107?q=80&w=240&h=360&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
              alt="Clean Bed"
              style={{
                width: 240,
                height: 360,
              }}
            />
          </Inset>
          <Heading align="center">Reserve</Heading>
        </Link>
      </Card>
    </Flex>
  );
}
