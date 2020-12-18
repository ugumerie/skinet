let data: number | string;

data = 42;
data = '42';

interface ICar {
    color: string;
    model: string;
    topSpeed?: number;
}

const car1: ICar = {
    color: 'blue',
    model: 'bmw'
};

const car2: ICar = {
    color: 'red',
    model: 'mercedes',
    topSpeed: 100
};
