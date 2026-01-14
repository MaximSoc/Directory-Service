"use client";

import { Button } from "@/components/ui/button";

export default function Counter() {
  const count = CalculateSum(10, 3);

  function CalculateSum(a: number, b: number) {
    return a + b;
  }

  const handleClick = () => {
    console.log(count);
  };

  return (
    <div className="flex flex-col gap-3">
      <span>Counter - {count}</span>
      <Button variant="destructive" onClick={handleClick} className="w-fit">
        Кнопка
      </Button>
    </div>
  );
}
