import { Producer } from 'k6/x/kafka';
import { check } from 'k6';
import encoding from 'k6/encoding';

const TOPIC = 'abs.message';

const producer = new Producer({
    brokers: ['localhost:9094'],
            topic: TOPIC,
});

const departmentIds = [
    "c367d653-4d63-482e-bc5e-3a253baeabcf",
    "145b5db8-049a-486f-95ba-b2402bc5e844",
    "e4803427-ae14-4c3a-ae57-4792caf4342c",
    "cc46df69-71ea-4a44-b5cf-13e11cad2327"
];

const counterpartyIds = [
    "7707083893", "7707083894", "6607083894",
    "6607083893", "5507083893"
];

function randomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function randomFrom(values) {
    return values[randomInt(0, values.length - 1)];
}

function generateTransaction() {
    const ndsRate = 22;
    const quantity = randomInt(1, 10);
    const unitPrice = randomInt(100, 5000);
    const amount = quantity * unitPrice;
    const ndsAmount = amount * ndsRate;

    return {
        messageId: crypto.randomUUID(),
        operationNumber: `OP-${Date.now()}-${Math.random().toString(36).substr(2, 6)}`,
        type: [0, 1, 2][Math.floor(Math.random() * 3)],
        operationDate: new Date().toISOString(),

        unitPrice: unitPrice,
        ndsRate: ndsRate,
        ndsAmount: ndsAmount,
        amount: amount,
        currencyCode: 'RUB',

        buyerInn: randomFrom(counterpartyIds),
        buyerKpp: "770701001",
        buyerName: "dadas",
        buyerAddress: "hdfhdh",
        sellerInn: "75675675",
        sellerKpp: "770701001",
        sellerName: "dadas",
        sellerAddress: "hdfhdh",

        productName: 'Какой-то продукт',
        productCode: Math.random().toString(36).substr(2, 6),
        unit: 'шт',
        quantity: quantity,

        contractNumber: "12423423423",
        contractDate: new Date().toISOString(),
        paymentDocumentNumber: "43125345235235345",
        operationType: "dsadada",

        departmentId: randomFrom(departmentIds),
        postedAt: new Date().toISOString()
    };
}

export const options = {
    scenarios: {
        target_throughput: {
            executor: 'constant-arrival-rate',
            rate: 120,
            timeUnit: '1s',
            duration: '3m',
            preAllocatedVUs: 10,
            maxVUs: 30,
        }
    }
};

export default function () {
    const transaction = generateTransaction();

    const err = producer.produce({
        topic: TOPIC,
    messages: [{
        key: encoding.b64encode(transaction.operationNumber),
        value: encoding.b64encode(JSON.stringify(transaction)),
    }]
    });

    check(err, {
        'message produced successfully': (e) => e === null,
    });
}

export function teardown() {
    producer.close();
}